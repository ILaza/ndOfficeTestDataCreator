using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace NetDocuments.Automation.Helpers
{
    // TODO: make this class static
    /// <summary>
    /// Holds methods to perform file checks.
    /// </summary>
    public class FileSystemHelper
    {
        /// <summary>
        /// Checks whether a directory is empty.
        /// </summary>
        /// <param name="path">Full path to the directory.</param>
        /// <returns>True if directory doesn't have files; otherwise, false.</returns>
        public bool IsDirectoryEmpty(string path) => !GetFiles(path).Any();

        /// <summary>
        /// Waits for file in the given path.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="fileName">The filename to wait.</param>
        public void WaitForFile(string path, string fileName)
        {
            Wait.For(() => FileExists(path, fileName));
        }

        /// <summary>
        /// Waits for file in the given path.
        /// </summary>
        /// <param name="fullPath">Path with file name.</param>
        public void WaitForFile(string fullPath)
        {
            Wait.For(() => FileExists(fullPath));
        }

        /// <summary>
        /// Returns the names of files (including their paths) that match the specified search
        /// pattern in the specified directory, using a value to determine whether to search
        /// subdirectories.
        /// </summary>
        /// <param name="path">
        /// The relative or absolute path to the directory to search. This string is not
        /// case-sensitive.
        /// </param>
        /// <param name="pattern">
        /// The search string to match against the names of files in the path. This parameter
        /// can contain a combination of valid literal path and wildcard (* and ?) characters,
        /// but doesn't support regular expressions.
        /// </param>
        /// <param name="option">
        /// One of the enumeration values that specifies whether the search operation should
        /// include all subdirectories or only the current directory.
        /// </param>
        /// <returns>
        /// An array of the full names (including paths) for the files in the specified directory
        /// that match the specified search pattern and option, or an empty array if no files are found.
        /// </returns>
        public string[] GetFiles(string path,
                                 string pattern = "*.*",
                                 SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return Directory.GetFiles(path, pattern, option);
        }

        /// <summary>
        /// Checks whether the specified file exists.
        /// </summary>
        /// <param name="fullPath">The full file path to check.</param>
        /// <returns>True if the specified file exists; otherwise, false.</returns>
        public bool FileExists(string fullPath) => File.Exists(fullPath);

        /// <summary>
        /// Checks whether the specified file exists in the given location.
        /// </summary>
        /// <param name="path">The path to folder to check.</param>
        /// <param name="fileName">The filename to find in the given path.</param>
        /// <returns>True if the specified file exists; otherwise, false.</returns>
        public bool FileExists(string path, string fileName)
        {
            var fullPath = Path.Combine(path, fileName);
            return FileExists(fullPath);
        }

        /// <summary>
        /// Deletes all files in the given folder.
        /// </summary>
        /// <param name="path">The full folder path to delete files.</param>
        public void DeleteFolderContent(string path)
        {
            if (Directory.Exists(path))
            {
                var files = GetFiles(path);
                Array.ForEach(files, file =>
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                });
            }
        }

        /// <summary>
        /// Deletes all folder content and subfolders if recursive or only folder content if no.
        /// </summary>
        /// <param name="path">Path to folder to delete.</param>
        /// <param name="recursive">Determines if all subfolders should be deleted.</param>
        public void DeleteFolder(string path, bool recursive)
        {
            if (recursive)
            {
                try
                {
                    Directory.Delete(path, recursive);
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Could not completely delete folder {path} because of {ex.Message}");
                    Console.WriteLine("Details:");
                    Console.WriteLine(ex);
                }
            }
            else
            {
                DeleteFolderContent(path);
            }
        }

        /// <summary>
        /// Determines if file has all attributes from collection.
        /// </summary>
        /// <param name="fullPath">The full file path to check.</param>
        /// <param name="attributesCollection">Profile attributes collection.</param>
        /// <returns>True if file has all given profile attributes. Otherwise false.</returns>
        public bool HasFileAttributes(string fullPath, IEnumerable<FileAttributes> attributesCollection)
        {
            var fileCheckResult = false;
            if (FileExists(fullPath))
            {
                var attributes = File.GetAttributes(fullPath);
                fileCheckResult = attributesCollection.All(attr => Convert.ToBoolean(attr & attributes));
            }
            return fileCheckResult;
        }

        /// <summary>
        /// Determines if file has specified attribute.
        /// </summary>
        /// <param name="fullPath">The full file path to check.</param>
        /// <param name="attribute">File attribute to check.</param>
        /// <returns>True if file exists and has specified attribute. Otherwise - false.</returns>
        public bool HasFileAttribute(string fullPath, FileAttributes attribute)
        {
            return HasFileAttributes(fullPath, new FileAttributes[] { attribute });
        }

        /// <summary>
        /// Checks is file in ReadOnly mode.
        /// </summary>
        /// <param name="fullPath">The full file path to check.</param>
        /// <returns>True if file exists and in ReadOnly mode. Otherwise - false.</returns>
        public bool IsFileReadOnly(string fullPath)
        {
            return HasFileAttribute(fullPath, FileAttributes.ReadOnly);
        }

        /// <summary>
        /// Replaces one file with another.
        /// </summary>
        /// <param name="destinationFullFileName">Full file name for delete.</param>
        /// <param name="sourceFullFileName">Full file name for move.</param>
        public void ReplaceFile(string destinationFullFileName, string sourceFullFileName)
        {
            File.Delete(destinationFullFileName);
            File.Move(sourceFullFileName, destinationFullFileName);
        }

        /// <summary>
        /// Gets document name using full path.
        /// </summary>
        /// <param name="fullPath">Full path to document.</param>
        /// <returns>Document name.</returns>
        public string GetDocNameFromPath(string fullPath)
        {
            var file = new FileInfo(fullPath);
            return file.Name;
        }

        /// <summary>
        /// Returns random full file name in user temp directory with given extension.
        /// </summary>
        /// <param name="extension">File extension.</param>
        /// <returns>Full file name in user temp directory.</returns>
        public string GetFullFilePathForTempFile(string extension)
        {
            return $"{Path.GetTempFileName()}.{extension}";
        }

        /// <summary>
        /// Creates file from stream in given location.
        /// </summary>
        /// <param name="stream">Stream with data.</param>
        /// <param name="fullFilePath">Full file path.</param>
        /// <returns>True, if file was created; otherwise false.</returns>
        public bool CreateFileFromStream(Stream stream, string fullFilePath)
        {
            bool fileCreated = false;
            using (var fileStream = new FileStream(fullFilePath, FileMode.Create))
            {
                try
                {
                    stream.CopyTo(fileStream);
                    fileStream.Flush();

                    fileCreated = true;
                }
                catch
                {
                }
            }

            return fileCreated;
        }

        /// <summary>
        /// Deletes given file.
        /// </summary>
        /// <param name="path">The full folder path to delete files.</param>
        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        /// <summary>
        /// Tries to delete file from FS.
        /// Waits till file is steel in use
        /// </summary>
        public async void DeleteFileAsync(string path, int maxWaitTimeMs = 30000, int retryIntervalMs = 100)
        {
            await Task.Run(() =>
            {
                return Wait.ForResult(() =>
                {
                    try
                    {
                        DeleteFile(path);
                        return true;
                    }
                    catch (IOException)
                    {
                        // In case of this exception file is steel used by another process.
                        return false;
                    }
                },
                maxWaitTimeMs,
                retryIntervalMs);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Reads file into bytes array.
        /// </summary>
        /// <param name="fullFileName">Full file path.</param>
        /// <returns>File data if read operation was successful; otherwise null.</returns>
        public byte[] ReadFile(string fullFileName)
        {
            try
            {
                return File.ReadAllBytes(fullFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Can't read file {fullFileName} because of: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Gets file extension without '.' symbol.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <returns>File extension if exists;</returns>
        public string GetFileExtension(string fileName)
                => Path.GetExtension(fileName).Replace(".", "");

        /// <summary>
        /// Creates new zip archive with given file.
        /// </summary>
        /// <param name="fullFileName">File for archiving.</param>
        /// <returns>Full path to zip archive.</returns>
        public string ZipFile(string fullFileName)
        {
            try
            {
                var zippedFileName = $"{Path.GetTempFileName()}.zip";
                using (var zipStream = new FileStream(zippedFileName, FileMode.Create))
                {
                    using (var arch = new ZipArchive(zipStream, ZipArchiveMode.Create))
                    {
                        var zipEntry = arch.CreateEntry(Path.GetFileName(fullFileName));
                        using (var entryStream = zipEntry.Open())
                        {
                            var data = ReadFile(fullFileName);
                            entryStream.Write(data, 0, data.Length);
                        }
                    }
                }
                return zippedFileName;
            }
            catch
            {
                Console.WriteLine($"Can't create zip archive from file: {fullFileName}");
                return null;
            }
        }
    }
}