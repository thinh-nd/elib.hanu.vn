using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Drawing;
using iTextSharp.text;
using iTextSharp.text.pdf;
using GhostscriptSharp;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace QML.Web.UI.Helpers
{
    public class PdfHelper
    {
        public static void Encrypt(string filePath)
        {
            if (Path.GetExtension(filePath) != ".pdf")
                throw new FormatException("Not a pdf file");


            PdfReader reader = new PdfReader(new FileStream(filePath, FileMode.Open));
            using (PdfStamper stamper = new PdfStamper(reader, new FileStream(filePath, FileMode.Open)))
            {
                stamper.SetEncryption(
                    null, //user password
                    null, //owner password
                    PdfWriter.ALLOW_SCREENREADERS,
                    PdfWriter.ENCRYPTION_AES_128
                );
            }
        }

        public static void Decrypt(string filePath)
        {
            if (Path.GetExtension(filePath) != ".pdf")
                throw new FormatException("Not a pdf file");


            PdfReader reader = new PdfReader(new FileStream(filePath, FileMode.Open));
            using (PdfStamper stamper = new PdfStamper(reader, new FileStream(filePath, FileMode.Open)))
            {
                stamper.SetEncryption(
                    null, //user password
                    null, //owner password
                    PdfWriter.ALLOW_SCREENREADERS | PdfWriter.ALLOW_COPY | PdfWriter.ALLOW_DEGRADED_PRINTING | PdfWriter.ALLOW_PRINTING,
                    PdfWriter.ENCRYPTION_AES_128
                );
            }
        }

        //Not allow by IIS
        public static bool ConvertToPdf(string filePath)
        {
            string[] supportTypes = { ".doc", ".docx", ".ppt", "pptx" };
            bool isFormatValid = false;
            for (int i = 0; i < supportTypes.Length; i++)
            {
                if (Path.GetExtension(filePath).ToLower() == supportTypes[i])
                {
                    isFormatValid = true;
                    break;
                }
            }

            if (isFormatValid)
            {
                Process p = new Process();
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.WorkingDirectory = HttpContext.Current.Server.MapPath("~/Resources");
                p.StartInfo.FileName = HttpContext.Current.Server.MapPath("~/officetopdf.exe");
                p.StartInfo.Arguments = Path.GetFileName(filePath) + " "
                    + Path.GetFileNameWithoutExtension(filePath) + ".pdf /print";

                p.Start();
                p.WaitForExit();
                p.Close();
            }

            return isFormatValid;
        }

        public static void ExtractPage(string sourcePdfPath, string outputPdfPath, int pageNumber)
        {
            PdfReader reader = null;
            iTextSharp.text.Document document = null;
            PdfCopy pdfCopyProvider = null;
            PdfImportedPage importedPage = null;

            try
            {
                // Intialize a new PdfReader instance with the contents of the source Pdf file:
                reader = new PdfReader(sourcePdfPath);

                // Capture the correct size and orientation for the page:
                document = new iTextSharp.text.Document(reader.GetPageSizeWithRotation(pageNumber));

                // Initialize an instance of the PdfCopyClass with the source 
                // document and an output file stream:
                pdfCopyProvider = new PdfCopy(document,
                    new System.IO.FileStream(outputPdfPath, System.IO.FileMode.Create));

                document.Open();

                // Extract the desired page number:
                importedPage = pdfCopyProvider.GetImportedPage(reader, pageNumber);
                pdfCopyProvider.AddPage(importedPage);
                document.Close();
                reader.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Example: ScaleTo(PageSize.A4, sourcePdfPath, desPdfPath)
        public static void ScaleTo(iTextSharp.text.Rectangle pageSize, string inPDF, string outPDF)
        {
            PdfReader reader = new PdfReader(inPDF);
            iTextSharp.text.Document doc = new iTextSharp.text.Document(pageSize);
            iTextSharp.text.Document.Compress = true;
            PdfWriter writer = PdfWriter.GetInstance(doc,
                new FileStream(outPDF, FileMode.Create));
            doc.Open();
            PdfContentByte cb = writer.DirectContent;

            PdfImportedPage page;
            for (int pageNumber = 1; pageNumber <= reader.NumberOfPages; pageNumber++)
            {
                page = writer.GetImportedPage(reader, pageNumber);

                if (page.Width <= page.Height)
                    doc.SetPageSize(pageSize);
                else
                    doc.SetPageSize(pageSize.Rotate());
                doc.NewPage();

                cb.AddTemplate(page,
                    doc.PageSize.Width / reader.GetPageSize(pageNumber).Width,
                    0, 0,
                    doc.PageSize.Height / reader.GetPageSize(pageNumber).Height,
                    0, 0);
            }
            doc.Close();
        }

        //Enable Win-32 Application on IIS
        public static string CreatePdfThumbnail(string pdfPath, string pngName)
        {
            string pngPath = HttpContext.Current.Server.MapPath("~/uploads/" + pngName);
            GhostscriptSettings setting = new GhostscriptSettings();
            setting.Device = GhostscriptSharp.Settings.GhostscriptDevices.pngalpha;
            setting.Page = new GhostscriptSharp.Settings.GhostscriptPages { Start = 1, End = 1, AllPages = false };
            setting.Resolution = new Size { Height = 72, Width = 72 }; //72dpi
            setting.Size = new GhostscriptSharp.Settings.GhostscriptPageSize { Manual = new Size { Height = 150, Width = 128 } };
            GhostscriptWrapper.GenerateOutput(pdfPath, pngPath, setting);

            return pngName;
        }

        //Trim every white space around an image, 
        public static void ImageTrim(string imagePath, string desPath)
        {
            //get image data
            Bitmap img = new Bitmap(imagePath);
            BitmapData bd = img.LockBits(new System.Drawing.Rectangle(Point.Empty, img.Size),
            ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int[] rgbValues = new int[img.Height * img.Width];
            Marshal.Copy(bd.Scan0, rgbValues, 0, rgbValues.Length);
            img.UnlockBits(bd);


            #region determine bounds
            int left = bd.Width;
            int top = bd.Height;
            int right = 0;
            int bottom = 0;

            //determine top
            for (int i = 0; i < rgbValues.Length; i++)
            {
                int color = rgbValues[i] & 0xffffff;
                if (color != 0xffffff)
                {
                    int r = i / bd.Width;
                    int c = i % bd.Width;

                    if (left > c)
                    {
                        left = c;
                    }
                    if (right < c)
                    {
                        right = c;
                    }
                    bottom = r;
                    top = r;
                    break;
                }
            }

            //determine bottom
            for (int i = rgbValues.Length - 1; i >= 0; i--)
            {
                int color = rgbValues[i] & 0xffffff;
                if (color != 0xffffff)
                {
                    int r = i / bd.Width;
                    int c = i % bd.Width;

                    if (left > c)
                    {
                        left = c;
                    }
                    if (right < c)
                    {
                        right = c;
                    }
                    bottom = r;
                    break;
                }
            }

            if (bottom > top)
            {
                for (int r = top + 1; r < bottom; r++)
                {
                    //determine left
                    for (int c = 0; c < left; c++)
                    {
                        int color = rgbValues[r * bd.Width + c] & 0xffffff;
                        if (color != 0xffffff)
                        {
                            if (left > c)
                            {
                                left = c;
                                break;
                            }
                        }
                    }

                    //determine right
                    for (int c = bd.Width - 1; c > right; c--)
                    {
                        int color = rgbValues[r * bd.Width + c] & 0xffffff;
                        if (color != 0xffffff)
                        {
                            if (right < c)
                            {
                                right = c;
                                break;
                            }
                        }
                    }
                }
            }

            int width = right - left + 1;
            int height = bottom - top + 1;
            #endregion

            //copy image data
            int[] imgData = new int[width * height];
            for (int r = top; r <= bottom; r++)
            {
                Array.Copy(rgbValues, r * bd.Width + left, imgData, (r - top) * width, width);
            }

            //create new image
            Bitmap newImage = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData nbd
                = newImage.LockBits(new System.Drawing.Rectangle(0, 0, width, height),
                    ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(imgData, 0, nbd.Scan0, imgData.Length);
            newImage.UnlockBits(nbd);
            newImage.Save(desPath, ImageFormat.Png);
        }
    }
}