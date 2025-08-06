using DemoProject_backend.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Microsoft.AspNetCore.Http.HttpResults;
using System.IO;
using System.Reflection.Metadata;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;
using static System.Net.Mime.MediaTypeNames;

namespace DemoProject_backend.Helper
{
    public class PdfCompressor
    {
        public static void SmartCompress(string src, string dest)
        {
            //Note: Wrapping with using() helps All file handles and memory are released properly.

            // this is a iTextSharp.text.pdf It reads an existing PDF file from the specified path It parses PDF structure like image, text. tables
            using (PdfReader reader = new PdfReader(src))


            //Creates an empty file at the destination path
            using (FileStream fs = new FileStream(dest, FileMode.Create))


            //Document defines the output PDF structure
            //reader.GetPageSizeWithRotation(1) ensures the page size &rotation matches the original's first page
            //This ensures proper layout/ format when copying pages
            // Here the 1 means the page number
            using (iTextSharp.text.Document doc = new iTextSharp.text.Document(reader.GetPageSizeWithRotation(1)))


            //Creates a PdfSmartCopy object, which is a helper class from iTextSharp.
            //This is used to copy pages from an existing PDF(reader) into a new one(doc).
            //The fs is a FileStream that writes the new PDF to disk.
            //PdfSmartCopy is smart because it avoids duplicating shared resources like fonts and images if they're reused across pages (saving space).
            using (PdfSmartCopy copy = new PdfSmartCopy(doc, fs))
            {

                //tells the copy tool to use the highest possible compression level.
                //PdfStream.BEST_COMPRESSION is a constant in iTextSharp(-1) that activates best ZIP-level compression for PDF streams (like images, fonts, etc.).
                copy.CompressionLevel = PdfStream.BEST_COMPRESSION;

                //Opens the new PDF Document (doc) for writing.
                doc.Open();

                for (int i = 1; i <= reader.NumberOfPages; i++)
                {

                    //GetImportedPage(reader, i) retrieves the page from the source file.
                    //AddPage(...) adds that page to the new document.
                    copy.AddPage(copy.GetImportedPage(reader, i));
                }
                //Closes the PDF Document properly.
                doc.Close();
            }
        }
    }
}

