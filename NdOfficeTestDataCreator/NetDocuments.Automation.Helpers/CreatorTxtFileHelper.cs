using System;
using System.Collections.Generic;
using System.IO;

namespace NetDocuments.Automation.Helpers
{
    public static class CreatorTxtFileHelper
    {
        /// <summary>
        /// Creates file.
        /// </summary>
        /// <param name="path">The path and name of the file to create.</param>
        /// <param name="fileSize">File size in bytes.</param>
        public static void CreateFile(string path, long fileSize)
        {
            try
            {
                using (var file = File.Create(path))
                {
                    var bytesList = new List<byte>();

                    for (int i = 0; i < fileSize; i++)
                    {
                        bytesList.Add(1);
                    }

                    file.Write(bytesList.ToArray(), 0, bytesList.Count);
                    file.Flush();
                    file.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Could not create new .txt file.", ex);
            }
        }

        /// <summary>
        /// Deletes file.
        /// </summary>
        /// <param name="path">The path and name of the file to delete.</param>
        public static void DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not create new .txt file.", ex);
            }
        }
    }
}
