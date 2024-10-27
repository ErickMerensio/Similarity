using Microsoft.Maui.Storage;
using OpenCvSharp;
using OpenCvSharp.Features2D;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Size = OpenCvSharp.Size;

namespace FingerprintComparisonApp
{
    public partial class MainPage : ContentPage
    {
        private DatabaseService dbService;
        private string imagePath1;

        public MainPage()
        {
            InitializeComponent();
            dbService = new DatabaseService();
        }

        private async Task<string> PickImageAsync()
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

        private async void OnSelectImage1Clicked(object sender, EventArgs e)
        {
            imagePath1 = await PickImageAsync();
            if (!string.IsNullOrEmpty(imagePath1))
            {
                imagePath1 = Path.GetFullPath(imagePath1);
                LabelImage1.Text = $"Imagem 1: {Path.GetFileName(imagePath1)}";
                ButtonCompare.IsEnabled = true;
            }
        }

        private async Task<Mat> CreateMatchImage(Mat img1, Mat img2, List<DMatch> goodMatches, KeyPoint[] keypoints1, KeyPoint[] keypoints2)
        {
            Mat matchImage = new Mat();
            Cv2.DrawMatches(img1, keypoints1, img2, keypoints2, goodMatches, matchImage, flags: DrawMatchesFlags.NotDrawSinglePoints);
            return matchImage;
        }

        private async Task<bool> CompareFingerprintsAndShowMatches(string imagePath1, string imagePath2)
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

                    // Aqui você define a imagem de comparação como a fonte para FullScreenImage
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

        private async void OnCompareFingerprintsClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(imagePath1))
            {
                var fingerprints = dbService.GetAllFingerprints();

                foreach (var fingerprint in fingerprints)
                {
                    // Compara a imagem selecionada com a imagem do banco de dados
                    bool isMatch = await CompareFingerprintsAndShowMatches(imagePath1, fingerprint.ImagePath);
                    if (isMatch)
                    {
                        string nome = fingerprint.Nome;
                        string cargo = fingerprint.Cargo;
                        await Task.Delay(4000);

                        // Navega para a página Teste após a visualização da imagem de comparação
                        await Navigation.PushAsync(new Teste(nome, cargo));
                        return; // Sai do loop após encontrar a primeira correspondência
                    }
                }
                await DisplayAlert("Acesso Negado!", "Impressão digital não encontrada no banco de dados.", "OK");
            }
        }

        private async void OnAddFingerprintClicked(object sender, EventArgs e)
        {
            string imagePath = await PickImageAsync(); // Seleciona uma imagem
            string nome = EntryNome.Text; // Certifique-se de que EntryNome está definido no XAML
            string cargo = CargoPicker.SelectedItem?.ToString(); // Certifique-se de que CargoPicker está definido no XAML
            if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(cargo))
            {
                await DisplayAlert("Erro", "Por favor, digite o nome e selecione o cargo.", "OK");
                return;
            }

            if (!string.IsNullOrEmpty(imagePath))
            {
                // Adiciona a impressão digital junto com o nome e o cargo ao banco de dados
                dbService.AddFingerprint(imagePath, nome, cargo);
                await DisplayAlert("Sucesso", "Impressão digital adicionada ao banco de dados.", "OK");
            }
            else
            {
                await DisplayAlert("Erro", "Por favor, selecione uma imagem.", "OK");
            }
        }
    }
}
