using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ImageShrinkifier
{
    class Program
    {
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        public static String[] GetFilesFrom(String searchFolder, String[] filters, bool isRecursive)
        {
            List<String> filesFound = new List<String>();
            var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var filter in filters)
            {                
                filesFound.AddRange(Directory.GetFiles(searchFolder, String.Format("*.{0}", filter), searchOption));
            }
            return filesFound.ToArray();
        }

        public static long Compression = 95L;

        public static int SuccessCount = 0;

        public static string WorkingDirectory = System.Environment.CurrentDirectory;

        public static string ShrinkifyTempFile = "ShrinkifyTempFile.jpg";

        public static void DeleteTempFile()
        {
            if (File.Exists(WorkingDirectory + ShrinkifyTempFile))
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                File.Delete(WorkingDirectory + ShrinkifyTempFile);
            }           
        }

        public static string GetFileName(string imgFile)
        {
            string path = imgFile.ToString();
            int pos = path.LastIndexOf("\\") + 1;
            string imgName = path.Substring(pos, path.Length - pos);
            return imgName;
        }

        public static string GetFileExtension(string imgFile)
        {
            string path = imgFile.ToString();
            int pos = path.LastIndexOf(".") + 1;
            string extension = path.Substring(pos, path.Length - pos);
            return extension;
        }

        public static string MakeFolderPath()
        {
            //this is where the compressed images are saved
            string folderPath = WorkingDirectory + "/readyForSIP/";          
            new FileInfo(folderPath).Directory.Create();           
            return folderPath;
        }

    public static void ShrinkifyImage(string imgFile)
        {
            using (Bitmap imgsrc = new Bitmap(imgFile))
            {
                DeleteTempFile();
           
                    ImageCodecInfo imgEncoder = GetEncoder(ImageFormat.Jpeg);

                    // Create an Encoder object based on the GUID  
                    // for the Quality parameter category.  
                    System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                    EncoderParameters myEncoderParameters = new EncoderParameters(1);
                    EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, Compression);
                    myEncoderParameters.Param[0] = myEncoderParameter;
                    imgsrc.Save(@WorkingDirectory + ShrinkifyTempFile, imgEncoder, myEncoderParameters);

                    long length = new System.IO.FileInfo(WorkingDirectory + (ShrinkifyTempFile)).Length;
                    if (length > 3145728)
                    {
                        Compression--;
                        ShrinkifyImage(imgFile);
                    }
                    else
                    {
                        string folderPath = MakeFolderPath();
                        string imgName = GetFileName(imgFile);
                        try
                        {
                            imgsrc.Save(@folderPath + imgName, imgEncoder, myEncoderParameters);
                            Console.WriteLine("Compressed " + imgName);
                            SuccessCount++;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Could not compress " + imgName + " Error: " + e.Message.ToString());
                        }
                        return;
                    }
                }
            }
       

static void Main(string[] args)
        {
            
            //find all the image files in the folder
            String searchFolder = WorkingDirectory;
            var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp" };
            var files = GetFilesFrom(searchFolder, filters, false);

            //one at a time process an image file
            foreach(var imgFile in files)
            {
                //reset the compression
                Compression = 95L;

                string imgName = GetFileName(imgFile);
                long length = new System.IO.FileInfo(imgFile.ToString()).Length;
                if (length < 3145728)
                {
                    try
                    {
                        string folderPath = MakeFolderPath();
                        File.Copy(imgFile.ToString(), folderPath + imgName);
                        Console.WriteLine("Copied " + imgName);
                        SuccessCount++;
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Could not copy " + imgName + " Error: " + e.Message.ToString());
                    }
                } else if (imgName != ShrinkifyTempFile)
                {
                    ShrinkifyImage(imgFile.ToString());
                }               
            }
            DeleteTempFile();
            
            Console.WriteLine("Successfully touched " + SuccessCount + " of " + files.Count());
            Console.ReadKey();
        }
    }
}
