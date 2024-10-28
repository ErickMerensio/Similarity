using Microsoft.Maui.Storage;
using OpenCvSharp;
using OpenCvSharp.Features2D;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Size = OpenCvSharp.Size;
using Plugin.Fingerprint.Abstractions;

namespace FingerprintComparisonApp;

public partial class Login : ContentPage
{
    private readonly DatabaseService dbService;
    private readonly IFingerprint fingerprint;
    private string selectedImagePath;
    private string imagePath1;

    public Login()
    {
        InitializeComponent();
        dbService = DatabaseService.Instance;
        this.fingerprint = fingerprint;
    }

#if ANDROID
    async void OnBiometricClicked(object sender, EventArgs e)
    {
        var request = new AuthenticationRequestConfiguration("Validate that you have fingers", "Because without them you will not be able to access");
        var result = await fingerprint.AuthenticateAsync(request);
        if (result.Authenticated)
        {
            await DisplayAlert("Authenticate!", "Access Granted", "OK");
        }
        else
        {
            await DisplayAlert("Unauthenticated", "Access Denied", "OK");
        }
    }
#else
    void OnBiometricClicked(object sender, EventArgs e)
    {       
    }
#endif

    async Task<string> PickImageAsync()
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Escolha uma impressão digital",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                return result.FullPath;
            }
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    async Task<Mat> CreateMatchImage(Mat img1, Mat img2, List<DMatch> goodMatches, KeyPoint[] keypoints1, KeyPoint[] keypoints2)
    {
        Mat matchImage = new Mat();
        Cv2.DrawMatches(img1, keypoints1, img2, keypoints2, goodMatches, matchImage, flags: DrawMatchesFlags.NotDrawSinglePoints);
        return matchImage;
    }

    async void OnSelectImage1Clicked(object sender, EventArgs e)
    {
        imagePath1 = await PickImageAsync();
        if (!string.IsNullOrEmpty(imagePath1))
        {
            imagePath1 = Path.GetFullPath(imagePath1);
            LabelImage1.Text = $"Imagem 1: {Path.GetFileName(imagePath1)}";
            ButtonCompare.IsEnabled = true;
        }
    }

    async Task<bool> CompareFingerprintsAndShowMatches(string imagePath1, string imagePath2)
    {
        try
        {
            Mat img1 = Cv2.ImRead(imagePath1, ImreadModes.Grayscale);
            Mat img2 = Cv2.ImRead(imagePath2, ImreadModes.Grayscale);

            if (img1.Empty() || img2.Empty())
            {
                return false;
            }

            // Pré-processamento das imagens
            Cv2.GaussianBlur(img1, img1, new Size(5, 5), 0);
            Cv2.GaussianBlur(img2, img2, new Size(5, 5), 0);
            Cv2.EqualizeHist(img1, img1);
            Cv2.EqualizeHist(img2, img2);

            // Inicializa o detector SIFT
            var sift = SIFT.Create();
            KeyPoint[] keypoints1, keypoints2;
            Mat descriptors1 = new Mat(), descriptors2 = new Mat();
            sift.DetectAndCompute(img1, null, out keypoints1, descriptors1);
            sift.DetectAndCompute(img2, null, out keypoints2, descriptors2);

            var bf = new BFMatcher(NormTypes.L2, crossCheck: false);
            var matches = bf.KnnMatch(descriptors1, descriptors2, k: 2);

            List<DMatch> goodMatches = new List<DMatch>();
            foreach (var match in matches)
            {
                if (match[0].Distance < 0.75 * match[1].Distance)
                {
                    goodMatches.Add(match[0]);
                }
            }

            double matchPercentage = goodMatches.Count / (double)keypoints1.Length;
            if (matchPercentage > 0.15) // Ajuste conforme necessário
            {
                Mat matchImage = await CreateMatchImage(img1, img2, goodMatches, keypoints1, keypoints2);
                string matchImagePath = Path.Combine(FileSystem.CacheDirectory, "matchImage.jpg");
                matchImage.SaveImage(matchImagePath);

                // Define a imagem de comparação como a fonte para FullScreenImage
                FullScreenImage.Source = ImageSource.FromFile(matchImagePath);
                FullScreenImage.IsVisible = true; // Torna a imagem visível
                MainContentLayout.IsVisible = false; // Oculta o layout principal

                return true; // Indica que houve uma correspondência
            }
            return false; // Nenhuma correspondência encontrada
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao comparar as impressões digitais: {ex.Message}", "OK");
            return false;
        }
    }

    async void OnCompareFingerprintsClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(EntryNome.Text))
        {
            await DisplayAlert("Erro", "Por favor, digite seu nome.", "OK");
            return;
        }

        if (!string.IsNullOrEmpty(imagePath1))
        {
            var fingerprints = dbService.GetAllFingerprints();
            bool accessGranted = false;

            foreach (var fingerprint in fingerprints)
            {
                if (fingerprint.Nome == EntryNome.Text)
                {
                    bool isMatch = await CompareFingerprintsAndShowMatches(imagePath1, fingerprint.ImagePath);
                    if (isMatch)
                    {
                        accessGranted = true;
                        await Task.Delay(4000);
                        await Navigation.PushAsync(new AcessoLiberado(EntryNome.Text, fingerprint.Cargo));
                        return;
                    }
                }
            }

            if (!accessGranted)
            {
                await DisplayAlert("Acesso Negado!", "Impressão digital ou nome não encontrado no banco de dados.", "OK");
                FullScreenImage.IsVisible = false;
            }
        }
    }
}
