using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Dnn;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;

namespace YourNamespace.Controllers // Replace 'YourNamespace' with your actual namespace
{
    public class ObjectRecognitionController : Controller
    {
        private readonly string yoloConfigPath = @"C:\Users\enbys\Downloads\Object_Recognition\Object_Recognition\Assets\yolo\yolov3.cfg";
        private readonly string yoloWeightsPath = @"C:\Users\enbys\Downloads\Object_Recognition\Object_Recognition\Assets\yolo\yolov3.weights";
        private readonly string cocoNamesPath = @"C:\Users\enbys\Downloads\Object_Recognition\Object_Recognition\Assets\yolo\coco.names";

        private Net yoloNet;
        private List<string> classLabels;

        public ObjectRecognitionController()
        {
            // Load YOLO network
            yoloNet = DnnInvoke.ReadNetFromDarknet(yoloConfigPath, yoloWeightsPath);
            yoloNet.SetPreferableBackend(Emgu.CV.Dnn.Backend.OpenCV);
            yoloNet.SetPreferableTarget(Emgu.CV.Dnn.Target.Cpu); // Using CPU

            // Load class labels
            classLabels = System.IO.File.ReadAllLines(cocoNamesPath).ToList();
        }

        [HttpGet]
        public IActionResult ObjectRecognition()
        {
            return View("ObjectRecognition"); // Return the view
        }

        [HttpGet]
        public IActionResult GetFrame()
{
    using (VideoCapture capture = new VideoCapture(0)) // Default webcam
    {
        if (!capture.IsOpened)
        {
            return NotFound("Camera not available.");
        }

        using (Mat inputImage = new Mat())
        {
            capture.Read(inputImage);
            if (inputImage.IsEmpty)
                return NotFound("No frame captured.");

            // Prepare image for YOLO
            Mat inputBlob = DnnInvoke.BlobFromImage(inputImage, 1 / 255.0, new Size(416, 416), new MCvScalar(0, 0, 0), true, false);
            yoloNet.SetInput(inputBlob);

            // Get output layer names
            string[] outputLayerNames = yoloNet.UnconnectedOutLayersNames;

            // YOLO forward pass
            VectorOfMat outputMats = new VectorOfMat();
            yoloNet.Forward(outputMats, outputLayerNames);

            List<Rectangle> boxes = new List<Rectangle>();
            List<int> classIds = new List<int>();
            List<float> confidences = new List<float>();

            float confidenceThreshold = 0.5f;
            float nmsThreshold = 0.4f;

            // Process YOLO outputs
            for (int i = 0; i < outputMats.Size; i++)
            {
                Mat detectionMat = outputMats[i];
                for (int j = 0; j < detectionMat.Rows; j++)
                {
                    float[] row = new float[detectionMat.Cols];
                    detectionMat.Row(j).CopyTo(row);

                    float confidence = row[4]; // Object confidence score
                    if (confidence > confidenceThreshold)
                    {
                        float[] classScores = row.Skip(5).ToArray();
                        int classId = Array.IndexOf(classScores, classScores.Max());
                        float classConfidence = classScores[classId];

                        if (classConfidence > confidenceThreshold)
                        {
                            int centerX = (int)(row[0] * inputImage.Width);
                            int centerY = (int)(row[1] * inputImage.Height);
                            int width = (int)(row[2] * inputImage.Width);
                            int height = (int)(row[3] * inputImage.Height);

                            int x = centerX - (width / 2);
                            int y = centerY - (height / 2);

                            boxes.Add(new Rectangle(x, y, width, height));
                            classIds.Add(classId);
                            confidences.Add(confidence);
                        }
                    }
                }
            }

            // Non-maxima suppression
            int[] indices = DnnInvoke.NMSBoxes(boxes.ToArray(), confidences.ToArray(), confidenceThreshold, nmsThreshold);

            // Draw bounding boxes
            for (int i = 0; i < indices.Length; i++)
            {
                int idx = indices[i];
                Rectangle box = boxes[idx];
                CvInvoke.Rectangle(inputImage, box, new MCvScalar(0, 255, 0), 2);

                string label = $"{classLabels[classIds[idx]]}: {confidences[idx]:0.00}";
                CvInvoke.PutText(inputImage, label, new Point(box.X, box.Y - 10), FontFace.HersheySimplex, 0.5, new MCvScalar(255, 255, 255), 1);
            }

            // Convert Mat to Bitmap and return as image/jpeg
            using (MemoryStream ms = new MemoryStream())
            {
                using (Bitmap bitmap = inputImage.ToBitmap())
                {
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                }

                return File(ms.ToArray(), "image/jpeg");
            }
        }
    }
}



    }
}
