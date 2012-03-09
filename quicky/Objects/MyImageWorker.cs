using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Net;
using System.Drawing.Imaging;

namespace quicky.Objects
{
    internal class MyImageWorker
    {
        internal static string myPath = (System.Windows.Forms.Application.UserAppDataPath + @"\Icons\").ToLower();


      internal static string RelocateIconImage(string locationSourcefile)
        {
            if (string.IsNullOrWhiteSpace(locationSourcefile))
                return "";


            try
            {
                Uri path = new Uri(locationSourcefile);
                if (!path.IsLoopback)
                {
                    // it is not recomended to keep using the intenetfile to we go and fetch it, and save it local
                    try
                    {
                     string x =   locationSourcefile = DownloadImageAndSaveinTemp(path);
                     if (File.Exists(x))
                         locationSourcefile = x;
                    }
                    catch (Exception)
                    {
                        if (path != null)
                            throw;
                    }
                    //if file has been copyed local, we can work on it
                }
            }
            catch (Exception)
            {

                //if we fail we handle it as a normal file
            }



            string ttemppath = (Path.GetDirectoryName(locationSourcefile) + @"\").ToLower();
            if (Path.Equals(ttemppath.Replace("/", "\\"), myPath.Replace("/", "\\"))) //if the user provides a picture from the library we use it withouth checking
                return locationSourcefile;


            try
            {
                string newimageLocation = myPath + System.IO.Path.GetFileNameWithoutExtension(locationSourcefile) + ".png";

                // creat directory
                System.IO.Directory.CreateDirectory(myPath);

                Size maxiconSize = new Size()
                {
                    Width = 128,
                    Height = 128
                };
                Bitmap icon = new Bitmap(locationSourcefile);
                Size iconSyze = icon.Size;



                Bitmap result = new Bitmap(maxiconSize.Width, maxiconSize.Height);
                using (Graphics g = Graphics.FromImage((Image)result))
                {
                    g.DrawImage(icon, 0, 0, maxiconSize.Width, maxiconSize.Height);

                    if (File.Exists(newimageLocation)) //If the file already exists look if it is the same file
                    {

                        //Save new file temperaly to temp folder
                        string tempPath = Path.GetTempPath() + "temp.png";
                        result.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);
                        //Check to see if images are equal
                        if (ByteArrayCompare(GetData(tempPath), GetData(newimageLocation))) // 2 image are equal, do nothing
                        {
                            //clean temp png
                            File.Delete(tempPath);
                            return newimageLocation;
                        }
                        File.Delete(tempPath);
                        newimageLocation = getNewName(newimageLocation);
                    }

                    //We can not overwrite the existing image as they are not same, so give image a new name

                    result.Save(newimageLocation, System.Drawing.Imaging.ImageFormat.Png);
                    return newimageLocation;
                }
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
        }

        internal static string getNewName(string newimageLocation)
        {
            string temp = newimageLocation;
            int i = 0;
            do
            {
                temp = Path.GetDirectoryName(newimageLocation) + "\\" + Path.GetFileNameWithoutExtension(newimageLocation) + i++ + ".png";
            } while (File.Exists(temp) && ByteArrayCompare(GetData(temp), GetData(newimageLocation)));
            return temp;
        }

        internal static bool ByteArrayCompare(byte[] one, byte[] two)
        {
            if (one == null || two == null)
                return false;

            for (int i = 0; i < one.LongLength; i++)
            {
                if (!one[i].Equals(two[i]))
                {
                    return false;
                }
            }
            return true;
        }


        internal static byte[] GetData(string url)
        {
            byte[] x = null;
            Stream stream = null;
            try
            {
                using (stream = WebRequest.Create(url).GetResponse().GetResponseStream())
                {
                    x = ReadStream(stream, 0);

                }
            }
            catch (Exception)
            {

                return x;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
            return x;
        }

        internal static byte[] ReadStream(Stream stream, int initialLength)
        {
            int chunk;
            if (initialLength < 1)
            {
                initialLength = 0x8000;
            }
            byte[] buffer = new byte[initialLength];
            int read = 0;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;
                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();
                    if (nextByte == -1)
                    {
                        return buffer;
                    }
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            byte[] bytes = new byte[read];
            Array.Copy(buffer, bytes, read);
            return bytes;
        }

        internal static string DownloadImageAndSaveinTemp(Uri url)
        {
            try
            {
                string newLoc = Path.GetTempPath() + Path.GetFileName(url.LocalPath);
                WebRequest req = WebRequest.Create(url);
                WebResponse response = req.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    byte[] buffer = new byte[32 * 1024];
                    int read;
                    using (Stream output = new FileStream(newLoc, FileMode.Create))
                    {
                        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            output.Write(buffer, 0, read);
                        }
                        output.Close();
                    }
                    stream.Close();
                }

                return newLoc;
            }
            catch (Exception)
            {
                return null;
              

            }

        }

        internal static void CleanIconFolder()
        {
            if (Directory.Exists(myPath))
            {
                string[] images = Directory.GetFiles(myPath,"*");
                foreach (string image in images)
                {
                    if (!RepositoryLibrary.ExistsIconLocationRepository(image))
                    {
                        try
                        {
                            File.Delete(image);
                        }
                        catch (Exception)
                        {
                            
                           //if file is on lock or sothing do nothing, this is not an important feature
                        }
                    }
                }

            }
        }



    }
}
