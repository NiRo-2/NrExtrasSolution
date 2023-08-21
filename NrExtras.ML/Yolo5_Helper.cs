using Newtonsoft.Json;
using NrExtras.ExecuteTasks;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NrExtras.ML
{
    /// <summary>
    /// Yolo5 helper class which work together with ultralytics_yolov5 python project
    /// </summary>
    public class Yolo5_Helper
    {
        private string modelFilePath;
        private string ultralytics_yolov5_DirPath;
        private string detectPy_FilePath;

        //default fond for drawing on images
        private static System.Drawing.Font defaultFont = new System.Drawing.Font("Tahoma", 20);

        //prediction class
        public class Prediction
        {
            public int classNum { get; set; }
            public string label { get; set; }
            public double confidence { get; set; }
            public double x_Center { get; set; }
            public double y_Center { get; set; }
            public double width { get; set; }
            public double height { get; set; }
            public double topLeft_x { get; set; }
            public double topLeft_y { get; set; }
            public double bottomRight_x { get; set; }
            public double bottomRight_y { get; set; }
            //convtructor to parse dynamic object to our object
            public Prediction(dynamic dynamic)
            {
                classNum = dynamic.classNum;
                label = dynamic.label;
                confidence = double.Parse((string)dynamic.confidence);
                x_Center = dynamic.x;
                y_Center = dynamic.y;
                width = dynamic.w;
                height = dynamic.h;

                //auto calc prediction rectangle poins
                topLeft_x = x_Center - (width / 2);
                topLeft_y = y_Center - (height / 2);
                bottomRight_x = x_Center + (width / 2);
                bottomRight_y = y_Center + (height / 2);
            }
        }

        //image predictions class
        public class ImagePrediction
        {
            public string imageName { get; set; }
            public List<Prediction> predictions { get; set; } = new List<Prediction>();

            //constructor which get dynamic object and parse to out object
            public ImagePrediction(dynamic obj, double minConfidence)
            {
                imageName = obj.imageName;
                foreach (var item in obj.predictions)
                {
                    Prediction prediction = new Prediction(item);
                    if (prediction.confidence >= minConfidence) //incase we have enought confidence - add to our list
                        predictions.Add(new Prediction(item));
                }
            }
        }

        //constructor
        public Yolo5_Helper(string modelFilePath, string ultralytics_yolov5_DirPath)
        {
            //validating model and ultralytics_yolov5 dirPath
            if (!File.Exists(modelFilePath)) throw new Exception("Model file not found");
            if (!Directory.Exists(ultralytics_yolov5_DirPath)) throw new Exception("ultralytics_yolov5 dir not found");

            //set vals
            this.modelFilePath = modelFilePath;
            this.ultralytics_yolov5_DirPath = ultralytics_yolov5_DirPath;
            detectPy_FilePath = Path.Combine(this.ultralytics_yolov5_DirPath, "detect.py");
        }

        /// <summary>
        /// detect elements in inPath (can be dir or file) and return predictions list for each image
        /// </summary>
        /// <param name="inPath">directory holding images or image file to get predictions from</param>
        /// <param name="minConfidence">default 0.4 . minimum condfidence of predictions</param>
        /// <returns>list of predictions. list of each image with it's predictions</returns>
        public List<ImagePrediction> detectElements(string inPath, double minConfidence = 0.4)
        {
            try
            {
                //valid checks
                if (minConfidence < 0.1) throw new Exception("minConfidence must be larger then 0.1");
                if (!Directory.Exists(inPath) && !File.Exists(inPath)) //inPath can be file or directory
                    throw new Exception("inPath must dir or a file and must be exists of course");

                //detect and parse answer
                string detectOutput = ExecuteTasks.ExecuteTasks.ExecuteCommand_AndReadOutput($"python {detectPy_FilePath} --weights {modelFilePath} --source {inPath} --nosave --print_predictions_to_ui --conf {minConfidence}");
                List<object> results_List = JsonConvert.DeserializeObject<List<object>>(detectOutput);

                //valid empty check
                if (results_List == null || results_List.Count == 0) throw new Exception("No elements detected");

                //parse to our objects
                List<ImagePrediction> imagePredictions_List = new List<ImagePrediction>();
                foreach (object obj in results_List)
                    imagePredictions_List.Add(new ImagePrediction(obj, minConfidence));

                //return results
                return imagePredictions_List;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// count predictions on enitre list with option for limiting confidence
        /// </summary>
        /// <param name="imagePredictions">images predictions list</param>
        /// <param name="minConfidence">default is 0.1</param>
        /// <returns>predictions count for entire list</returns>
        public static int getTotalPredictionsCountForEntireList(List<ImagePrediction> imagePredictions, double minConfidence = 0.1)
        {
            //validate we have a list
            if (imagePredictions == null || imagePredictions.Count == 0) return 0;

            int totalPredictionsCount = 0;
            //go through all images and all predictions, if we have enough confidence --> count it in
            foreach (ImagePrediction imagePrediction in imagePredictions)
                foreach (Prediction prediction in imagePrediction.predictions)
                    if (prediction.confidence > minConfidence)
                        totalPredictionsCount++;

            //return results
            return totalPredictionsCount;
        }

        /// <summary>
        /// draw predictions to images
        /// </summary>
        /// <param name="inImagasDirPath">in images dir path</param>
        /// <param name="imagePredictions">images predictions list</param>
        /// <param name="outDirPath">out dir path. if empty, out dir will auto create in inImagesDirPath</param>
        /// <param name="rectangleBoundingColor">Red by default</param>
        /// <param name="textColor">Red by default</param>
        /// <param name="rectangleLineWith_pixels">3 by default</param>
        /// <param name="writeConfidence">true by default</param>
        /// <param name="font">Tahoma 20 by default</param>
        /// <exception cref="Exception">when error occurs</exception>
        public static void drawPredictionsToBitmap(string inImagasDirPath, List<ImagePrediction> imagePredictions, string outDirPath = "", Color? rectangleBoundingColor = null, Color? textColor = null, int rectangleLineWith_pixels = 3, bool writeConfidence = true, Font? font = null)
        {
            //valid inImagasDirPath path
            if (!Directory.Exists(inImagasDirPath)) throw new Exception("inImagasDirPath not found");

            //no outDir supplied - auto create in input dir path
            if (outDirPath.Length == 0)
                outDirPath = Path.Combine(inImagasDirPath, "out");

            //if out dir not exists - auto create it
            if (!Directory.Exists(outDirPath)) Directory.CreateDirectory(outDirPath);

            //go through all images and create image with prediction rect
            foreach (ImagePrediction imagePrediction in imagePredictions)
            {
                try
                {
                    //find image in inImagasDirPath
                    string originalImageFilePath = Path.Combine(inImagasDirPath, imagePrediction.imageName);
                    string outFilePath = Path.Combine(outDirPath, imagePrediction.imageName);

                    //if image not exists in inImagesDir - skip it
                    if (!File.Exists(originalImageFilePath)) throw new Exception($"{imagePrediction.imageName} image not found in {inImagasDirPath}");

                    //continue - image exists
                    Bitmap out_bitmap = new Bitmap(originalImageFilePath);

                    //set default if needed
                    if (rectangleBoundingColor == null) rectangleBoundingColor = System.Drawing.Color.Red;
                    if (textColor == null) textColor = System.Drawing.Color.Red;

                    //draw rectangles
                    foreach (Prediction prediction in imagePrediction.predictions)
                        using (Graphics gr = Graphics.FromImage(out_bitmap))
                        {
                            //get rectangle from prediction
                            Rectangle rectangle = new Rectangle((int)Math.Round(out_bitmap.Width * prediction.topLeft_x), (int)Math.Round(out_bitmap.Height * prediction.topLeft_y), (int)Math.Round(out_bitmap.Width * prediction.width), (int)Math.Round(out_bitmap.Height * prediction.height));
                            Pen thick_pen;
                            Brush brush;

                            //set rectangle and text
                            thick_pen = new Pen((System.Drawing.Color)textColor, rectangleLineWith_pixels);
                            brush = Brushes.Red;
                            if (font == null) font = defaultFont;

                            //draw rectangle
                            gr.DrawRectangle(thick_pen, rectangle);

                            //draw text
                            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                            gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                            StringFormat sf = new StringFormat();
                            sf.Alignment = StringAlignment.Center;
                            sf.LineAlignment = StringAlignment.Center;

                            string txt = prediction.label; //write label
                            if (writeConfidence) txt += " " + (prediction.confidence).ToString("n2"); //writing confidence or not
                            gr.DrawString(txt, font, brush, rectangle, sf);
                        }

                    //export
                    out_bitmap.Save(outFilePath);
                }
                catch
                {
                    throw;
                }
            }
        }
    }
}