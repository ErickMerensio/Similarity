using Microsoft.Maui.Storage;
using OpenCvSharp;
using OpenCvSharp.Features2D;
using System.Collections.Generic;
using Size = OpenCvSharp.Size;

namespace FingerprintComparisonApp
{
    public partial class MainPage : ContentPage
    {
        private DatabaseService dbService;
        private string imagePath1; // Variável global para armazenar o caminho da primeira imagem

        public MainPage()
        {
            InitializeComponent();
            dbService = new DatabaseService(); // Inicializa o serviço de banco de dados
        }

        private async Task<string> PickImageAsync()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Escolha uma impressão digital",
                    FileTypes = FilePickerFileType.Images // Define que só pode selecionar imagens
                });

                if (result != null)
                {
                    return result.FullPath; // Retorna o caminho completo da imagem selecionada
                }
                return null;
            }
            catch (Exception ex)
            {
                //await DisplayAlert("Erro", $"Falha ao selecionar a imagem: {ex.Message}", "OK");
                return null;
            }
        }

        private async void OnSelectImage1Clicked(object sender, EventArgs e)
        {
            imagePath1 = await PickImageAsync();  // Chama o método PickImageAsync para obter o caminho da imagem selecionada
            if (!string.IsNullOrEmpty(imagePath1))
            {
                // Converte o caminho da imagem para UTF-8
                imagePath1 = Path.GetFullPath(imagePath1);

                LabelImage1.Text = $"Imagem 1: {Path.GetFileName(imagePath1)}";  // Atualiza o rótulo (label) na interface
                ButtonCompare.IsEnabled = true;  // Habilita o botão de comparação
            }
        }

        private async void OnAddFingerprintClicked(object sender, EventArgs e)
        {
            string imagePath = await PickImageAsync(); // Seleciona uma imagem
            if (!string.IsNullOrEmpty(imagePath))
            {
                dbService.AddFingerprint(imagePath); // Adiciona ao banco de dados
                await DisplayAlert("Sucesso", "Impressão digital adicionada ao banco de dados.", "OK");
            }
        }

        private async Task<bool> CompareFingerprints(string imagePath1, string imagePath2)
        {
            try
            {
                // Converte os caminhos das imagens para UTF-8
                imagePath1 = Path.GetFullPath(imagePath1);
                imagePath2 = Path.GetFullPath(imagePath2);

                // Carregar as duas imagens em escala de cinza
                Mat img1 = Cv2.ImRead(imagePath1, ImreadModes.Grayscale);
                Mat img2 = Cv2.ImRead(imagePath2, ImreadModes.Grayscale);

                // Verificar se as imagens foram carregadas corretamente
                if (img1.Empty() || img2.Empty())
                {
                    //await DisplayAlert("Erro", "Erro ao carregar as imagens. Verifique os caminhos dos arquivos.", "OK");
                    return false;
                }

                // Pré-processamento das imagens
                Cv2.GaussianBlur(img1, img1, new Size(5, 5), 0);
                Cv2.GaussianBlur(img2, img2, new Size(5, 5), 0);
                Cv2.EqualizeHist(img1, img1);
                Cv2.EqualizeHist(img2, img2);

                // Inicializar o detector SIFT
                var sift = SIFT.Create();

                // Detectar pontos-chave e descritores
                KeyPoint[] keypoints1, keypoints2;
                Mat descriptors1 = new Mat(), descriptors2 = new Mat();
                sift.DetectAndCompute(img1, null, out keypoints1, descriptors1);
                sift.DetectAndCompute(img2, null, out keypoints2, descriptors2);

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

                // Critério de compatibilidade
                double matchPercentage = goodMatches.Count / (double)keypoints1.Length;
                if (matchPercentage > 0.15) // Ajuste o valor conforme necessário
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //await DisplayAlert("Erro", $"Falha ao comparar as impressões digitais: {ex.Message}", "OK");
                return false;
            }
        }


        private async void OnCompareFingerprintsClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(imagePath1)) // Verifica se uma imagem foi selecionada
            {
                var fingerprints = dbService.GetAllFingerprints(); // Obtém todas as impressões digitais do banco

                foreach (var fingerprint in fingerprints)
                {
                    bool isMatch = await CompareFingerprints(imagePath1, fingerprint); // Compara com as impressões armazenadas
                    if (isMatch)
                    {

                        await Navigation.PushAsync(new Teste());
                        return;
                    }
                }

                await DisplayAlert("Acesso Negado!", "Impressão digital não encontrada no banco de dados.", "OK");
            }
        }
    }
}
