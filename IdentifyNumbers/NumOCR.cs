using OpenCvSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using System.Speech.Synthesis;
using Yolov8Net;
using System.Speech.Synthesis.TtsEngine;
using System.Speech.Recognition;
using System.Diagnostics;

namespace IdentifyNumbers {
    public class NumOCR {

        private string modelPath;
        private string imgPath;
        private string imgName;
        private string[] labelname;

        private const float MinConf = 0.1f;

        private string _result;
        public string result {
            get {
                return _result;
            }

            set {
                _result = value;
            }
        }

        public NumOCR(string imgPath, string imgName, string modelPath, string[] labelname) { 
            this.imgPath = imgPath;
            this.imgName = imgName;
            this.labelname = labelname;
            this.modelPath = modelPath;

            Run();  // Automatically Run Process
        }

        private void Run() {
            string file = Path.Combine(imgPath, imgName);

            if (System.IO.File.Exists(file)) {   // 경로에 이미지 파일이 있는지 확인
                using var yolo = YoloV8Predictor.Create(modelPath, labelname, false);

                Mat original_img = Cv2.ImRead(file, ImreadModes.Color);
                Mat processed_img = original_img;
                Mat resized_img = new Mat();
                using Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(file);

                int originalImageHeight = image.Height;
                int originalImageWidth = image.Width;

                List<List<int>> position_List = new List<List<int>>();
                Prediction[] predictions = yolo.Predict(image); // yolo.Predict()에 의해 image의 값이 바뀜

                foreach (Prediction pred in predictions) {
                    // Confidence(신뢰도)가 지정치(MinConf)보다 낮으면 Pass
                    if (pred.Score < MinConf) continue;

                    int predImageHeight = image.Height;
                    int predImageWidth = image.Width;

                    double x_scale = Math.Round((double)originalImageWidth / (double)predImageWidth, 3);
                    double y_scale = Math.Round((double)originalImageHeight / (double)predImageHeight, 3);

                    var x = Math.Max(pred.Rectangle.X, 0) * x_scale;
                    var y = Math.Max(pred.Rectangle.Y, 0) * y_scale;

                    var width = (pred.Rectangle.Width) * x_scale;
                    var height = (pred.Rectangle.Height) * y_scale;

                    position_List.Add(new List<int>() {
                        (int)x + (int)width / 2,            // [0] 가로 Center Point?
                        (int)y + (int)height / 2,           // [1] 세로 Center Point?
                        (int)width,                         // [2] Width
                        (int)height,                        // [3] Height
                        Convert.ToInt32(pred.Label.Name)    // [4]Label의 Class
                    }); ;

                    Cv2.Rectangle(processed_img, new OpenCvSharp.Point(x, y), new OpenCvSharp.Point(x + width, y + height), Scalar.Orange, 3, LineTypes.AntiAlias);
                    //Cv2.Rectangle(processed_img, new OpenCvSharp.Point(x, y - 20), new OpenCvSharp.Point(x + width, y), Scalar.Yellow, 3, LineTypes.AntiAlias);
                }
                position_List.Sort((a, b) => a[0].CompareTo(b[0])); // X를 기준으로 정렬
                

                for(int i = 0; i < position_List.Count; i++) result += $" {position_List[i][4]}";

                speech();

                Debug.WriteLine(result);

                // Boundary를 그린 Mat를 이미지 파일로 저장
                Cv2.ImWrite($"./img/bounded/bounded_{imgName}", processed_img);

                // ResizeImage Make
                Cv2.Resize(processed_img, resized_img, new OpenCvSharp.Size(processed_img.Width / 3, processed_img.Height / 3));
                Cv2.ImShow("display", resized_img);

                Cv2.WaitKey(0);

            } else return;    
        }

        private void speech() {
            SpeechSynthesizer voice = new SpeechSynthesizer();

            string[] members = result.Split(' ');
            string newString = string.Empty;
            foreach(string item in members) { 
                switch(item) {
                    case "10":
                        newString += ".";
                        break;
                    case "11":
                        newString += "";
                        break;
                    default:
                        newString += item;
                        break;
                }
            }
            Debug.WriteLine(newString);
            
            voice.Rate = -5;

            voice.SpeakAsync(newString);
        }
    }
}



//// X1: position_List[x][0] - position_List[x][2] / 2
//// Y1: position_List[x][1] - position_List[x][3] / 2
//// Width: position_List[x][2]
//// Height: position_List[x][3]
//// X2: position_List[x][0] - position_List[x][2] / 2 + position_List[x][2]
//// Y2: position_List[x][1] - position_List[x][3] / 2 + position_List[x][3]
//for (int i = 0; i < position_List.Count; i++) {
//    minX = Math.Min(minX, position_List[i][0] - position_List[i][2] / 2);
//    minY = Math.Min(minY, position_List[i][1] - position_List[i][3] / 2);

//    maxX = Math.Max(maxX, position_List[i][0] - position_List[i][2] / 2 + position_List[i][2]);
//    maxY = Math.Max(maxY, position_List[i][1] - position_List[i][3] / 2 + position_List[i][3]);
//}