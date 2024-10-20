using OpenCvSharp;
using OpenCvSharp.Features2D;
using Microsoft.Maui.Controls;
using System.IO;
using System.Collections.Generic;
using Size = OpenCvSharp.Size;

namespace FingerprintComparisonApp
{
    public partial class MainPage : ContentPage
    {
        private string imagePath1;
        private string imagePath2;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnSelectImage1Clicked(object sender, EventArgs e)
        {
            imagePath1 = await PickImageAsync();
            LabelImage1.Text = $"Imagem 1: {Path.GetFileName(imagePath1)}";
            CheckIfReadyToCompare();
        }

        private async void OnSelectImage2Clicked(object sender, EventArgs e)
        {
            imagePath2 = await PickImageAsync();
            LabelImage2.Text = $"Imagem 2: {Path.GetFileName(imagePath2)}";
            CheckIfReadyToCompare();
        }

        private void CheckIfReadyToCompare()
        {
            ButtonCompare.IsEnabled = !string.IsNullOrEmpty(imagePath1) && !string.IsNullOrEmpty(imagePath2);
        }

        private async void OnCompareFingerprintsClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(imagePath1) && !string.IsNullOrEmpty(imagePath2))
            {
                await CompareFingerprints(imagePath1, imagePath2);
            }
        }

        private async Task<string> PickImageAsync()
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Escolha uma impressão digital",
                FileTypes = FilePickerFileType.Images
            });

            return result?.FullPath;
        }

        private async Task CompareFingerprints(string imagePath1, string imagePath2)
        {
            Mat img1 = Cv2.ImRead(imagePath1, ImreadModes.Grayscale);
            Mat img2 = Cv2.ImRead(imagePath2, ImreadModes.Grayscale);

            // Verificar se as imagens foram carregadas corretamente
            if (img1.Empty() || img2.Empty())
            {
                await DisplayAlert("Erro", "Erro ao carregar as imagens. Verifique os caminhos dos arquivos.", "OK");
                return;
            }

            // Pré-processamento
            Cv2.GaussianBlur(img1, img1, new Size(5, 5), 0);
            Cv2.GaussianBlur(img2, img2, new Size(5, 5), 0);
            Cv2.EqualizeHist(img1, img1);
            Cv2.EqualizeHist(img2, img2);

            // Inicializar o detector SIFT
            var sift = SIFT.Create();

            // Detectar pontos-chave e descritores
            KeyPoint[] keypoints1;
            Mat descriptors1 = new Mat();
            sift.DetectAndCompute(img1, null, out keypoints1, descriptors1);  // Aqui, descriptors1 é passado como 'out'

            KeyPoint[] keypoints2;
            Mat descriptors2 = new Mat();
            sift.DetectAndCompute(img2, null, out keypoints2, descriptors2);  // Aqui, descriptors2 é passado como 'out'

            // Usar BFMatcher com o Ratio Test
            var bf = new BFMatcher(NormTypes.L2, crossCheck: false);
            var matches = bf.KnnMatch(descriptors1, descriptors2, k: 2);

            // Aplicar o Ratio Test
            List<DMatch> goodMatches = new List<DMatch>();
            foreach (var match in matches)
            {
                if (match[0].Distance < 0.75 * match[1].Distance)
                {
                    goodMatches.Add(match[0]);
                }
            }

            // Avaliar se as impressões digitais são compatíveis
            if (goodMatches.Count / (double)keypoints1.Length > 0.15) // Ajuste conforme necessário
            {
                await DisplayAlert("Resultado", "As impressões digitais são compatíveis!", "OK");
            }
            else
            {
                await DisplayAlert("Resultado", "As impressões digitais NÃO são compatíveis.", "OK");
            }

            // Desenhar as correspondências (opcional)
            Mat imgMatches = new Mat();
            Cv2.DrawMatches(img1, keypoints1, img2, keypoints2, goodMatches, imgMatches);
            Cv2.ImShow("Correspondências", imgMatches);
            Cv2.WaitKey(0);
            Cv2.DestroyAllWindows();
        }
    }
}
  
