namespace NrExtras.IO_Helper
{
    public static class IO_Helper
    {
        /// <summary>
        /// Get all files by extention from dir
        /// </summary>
        /// <param name="searchFolder">dir path</param>
        /// <param name="filters">array of filters. example: { "jpg", "jpeg", "png", "gif", "tiff", "bmp", "svg" }</param>
        /// <param name="searchInSubDirs">search in subdirs. true by default</param>
        /// <returns>list of found files</returns>
        public static List<string> GetFilesFromDirByExtenion(string searchFolder, string[] filters, bool searchInSubDirs = true)
        {
            List<string> filesFound = new List<string>();
            var searchOption = searchInSubDirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var filter in filters)
            {
                filesFound.AddRange(Directory.GetFiles(searchFolder, string.Format("*.{0}", filter), searchOption));
            }
            return filesFound.ToList();
        }
        /// <summary>
        /// Get list of all images from dir
        /// </summary>
        /// <param name="dir">dir path</param>
        /// <param name="searchInSubDirs">if true, search in all subdirs as well. true by default</param>
        /// <returns>list of strings of all images from dir</returns>
        public static List<string> GetAllImagesFromDir(string dir, bool searchInSubDirs = true)
        {
            string[] filters = new string[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp", "svg" };
            return GetFilesFromDirByExtenion(dir, filters, searchInSubDirs);
        }

        /// <summary>
        /// Validate file name dont have illegal chars in it. this can prevent hackers from going to places no excpected.
        /// ** this function should be call whenever we get filename from user - we have to validate he doesn't plant "..", "\\" chars in it. **
        /// </summary>
        /// <param name="fileName">original file name</param>
        /// <param name="throwExceptionOnInvalidChars">false by default. if true and invalid chars is found, throw exception</param>
        /// <param name="failOnEmptyStringResult">true by default, in case file name was "..", result string would be empty and if not handles right, could also cause un wanted path at the end</param>
        /// <returns>ligalyze string without illegal chars in it</returns>
        /// <exception cref="Exception">in case result string is empty</exception>
        public static string LegalizeFileName(string fileName, bool throwExceptionOnInvalidChars = false, bool failOnEmptyStringResult = true)
        {
            var invalids = Path.GetInvalidFileNameChars();
            var newName = string.Join("_", fileName.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');

            //fail on empty result
            if (failOnEmptyStringResult)
                if (string.IsNullOrEmpty(newName)) throw new Exception("Empty file name");

            //incase we want to threw exception if invalid char found
            if (throwExceptionOnInvalidChars)
                if (newName != fileName)
                    throw new Exception("Invalid chars found");

            //return new file name
            return newName;
        }

        /// <summary>
        /// Copy all dir content to another
        /// </summary>
        /// <param name="inDirPath">in dir path</param>
        /// <param name="outDirPath">out dir path</param>
        /// <param name="overwrite">true b default. overwrite out files..?</param>
        public static void copyDirContentToAnother(string inDirPath, string outDirPath, bool overwrite = true)
        {
            try
            {
                foreach (string dir in Directory.GetDirectories(inDirPath, "*", SearchOption.AllDirectories))
                    Directory.CreateDirectory(dir.Replace(inDirPath, outDirPath));

                //copy all files
                foreach (string newPath in Directory.GetFiles(inDirPath, "*.*", SearchOption.AllDirectories))
                    File.Copy(newPath, newPath.Replace(inDirPath, outDirPath), overwrite);
            }
            catch
            {
                throw;
            }
        }
    }
}