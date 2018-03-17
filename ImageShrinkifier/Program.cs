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
    class BatchShrink
    {

        private string subFolderPath = "";

        private long Compression = 95L;

        private int SuccessCount = 0;

        private string WorkingDirectory = System.Environment.CurrentDirectory;

        private string ShrinkifyTempFile = "ShrinkifyTempFile.jpg";

        private string[] searchFilters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp" };

        private string[] imgFiles;

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

        
        private bool DeleteTempFile()
        {
            if (File.Exists(WorkingDirectory + ShrinkifyTempFile))
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                File.Delete(WorkingDirectory + ShrinkifyTempFile);
                return true;
            }
            return false;
        }

        private void DeleteOriginalImgFiles()
        {
            foreach (var file in this.imgFiles)
            {
                if (File.Exists(file))
                {
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    File.Delete(file);
                }
            }
        }

        private string GetFileName(string imgFile)
        {
            string path = imgFile.ToString();
            int pos = path.LastIndexOf("\\") + 1;
            string imgName = path.Substring(pos, path.Length - pos);
            return imgName;
        }

        private string GetFileExtension(string imgFile)
        {
            string path = imgFile.ToString();
            int pos = path.LastIndexOf(".") + 1;
            string extension = path.Substring(pos, path.Length - pos);
            return extension;
        }

        private string MakeSubFolderPath()
        {
            //this is where the compressed images are saved
            string siteNumber = "";
            Console.WriteLine("Enter site number ");
            siteNumber = Console.ReadLine();
            if(siteNumber.Length < 5 )
            {
                siteNumber = "0" + siteNumber;
            }
            string folderPath = WorkingDirectory + "//" + siteNumber + "//";          
            new FileInfo(folderPath).Directory.Create();           
            return folderPath;
        }
        private bool copyImgFile(string imgName)
        {
            File.Copy(imgName, this.subFolderPath + imgName);
            return true;
        }
          
        private bool ShrinkifyImage(string imgFileName)
        {
            using (Bitmap imgsrc = new Bitmap(imgFileName))
            {
                this.DeleteTempFile();
           
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
                        this.ShrinkifyImage(imgFileName);
                    }
                    else
                    {                       
                        try
                        {
                            string path = this.subFolderPath;
                            imgsrc.Save(@path + imgFileName, imgEncoder, myEncoderParameters);
                            Console.WriteLine("Compressed " + imgFileName);
                            SuccessCount++;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Could not compress " + imgFileName + " Error: " + e.Message.ToString());
                        }
                        return false;
                    }
                }
            return true;
            }
       

static void Main(string[] args)
        {

            BatchShrink bs = new BatchShrink();

            //find all the image files in the folder            
            bs.imgFiles = GetFilesFrom(bs.WorkingDirectory, bs.searchFilters, false);

            //make sub folder to save the processed images
            bs.subFolderPath = bs.MakeSubFolderPath();

            //one at a time process an image file
            foreach (var imgFile in bs.imgFiles)
            {
                //reset the compression
                bs.Compression = 95L;

                string imgFileName = bs.GetFileName(imgFile);
                long imgFileSize = new System.IO.FileInfo(imgFileName).Length;
                if (imgFileSize > 3145728)
                {
                    if (imgFileName != bs.ShrinkifyTempFile)
                    {
                        bs.ShrinkifyImage(imgFileName);
                    }
                }
                else
                {
                    try
                    {
                        bs.copyImgFile(imgFileName);                        
                        Console.WriteLine("Copied " + imgFileName);
                        bs.SuccessCount++;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Could not copy " + imgFileName + " Error: " + e.Message.ToString());
                    }
                }                 
            }

            bs.DeleteTempFile();
            bs.DeleteOriginalImgFiles();
            Console.WriteLine("Successfully touched " + bs.SuccessCount + " of " + bs.imgFiles.Count());
            Console.ReadKey();
        }
    }
}
