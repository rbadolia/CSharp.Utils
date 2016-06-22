using System;
using System.Collections.Generic;
using System.IO;

namespace CSharp.Utils.IO
{
    public static class FindReplaceHelper
    {
        #region Public Methods and Operators

        public static string ReplaceDirectoryNames(string directoryName, string oldText, string newText)
        {
            string newDirectoryName = directoryName.Replace(oldText, newText);
            if (directoryName != newDirectoryName)
            {
                Directory.Move(directoryName, newDirectoryName);
            }

            string[] directories = Directory.GetDirectories(newDirectoryName);
            foreach (string directory in directories)
            {
                ReplaceDirectoryNames(directory, oldText, newText);
            }

            return newDirectoryName;
        }

        public static string ReplaceInDirectoriesAndFiles(string directoryName, string oldText, string newText, 
            bool renameDirectories, bool renameFiles, bool replaceFileContents, params string[] filePatterns)
        {
            if (!renameFiles && !renameDirectories && !replaceFileContents)
            {
                throw new ArgumentException(
                    "Any one of renameFiles, renameDirectories, replaceFileContents should be true");
            }

            directoryName = renameDirectories ? ReplaceDirectoryNames(directoryName, oldText, newText) : directoryName;
            string[] directories = Directory.GetDirectories(directoryName, "*", SearchOption.AllDirectories);
            List<string> fileNames;
            if (renameFiles)
            {
                fileNames = RenameFiles(directoryName, oldText, newText, filePatterns);
            }
            else
            {
                fileNames = GetAllFilesFromDirectory(directoryName, filePatterns);
            }

            if (replaceFileContents)
            {
                ReplaceInFiles(fileNames, oldText, newText);
            }

            return directoryName;
        }

        public static void ReplaceInFile(string fileName, string oldText, string newText)
        {
            string fileContent = File.ReadAllText(fileName);
            fileContent = fileContent.Replace(oldText, newText);
            File.WriteAllText(fileName, fileContent);
        }

        public static void ReplaceInFiles(IEnumerable<string> fileNames, string oldText, string newText)
        {
            foreach (var fileName in fileNames)
            {
                ReplaceInFile(fileName, oldText, newText);
            }
        }

        public static List<string> RenameFiles(string directoryName, string oldText, string newText, 
            params string[] filePatterns)
        {
            var fileNames = GetAllFilesFromDirectory(directoryName, filePatterns);
            return RenameFiles(fileNames, oldText, newText);
        }

        public static List<string> RenameFiles(IEnumerable<string> fileNames, string oldText, string newText)
        {
            var newList = new List<string>();
            foreach (var fileName in fileNames)
            {
                string newFileName = fileName.Replace(oldText, newText);
                if (fileName != newFileName)
                {
                    File.Move(fileName, newFileName);
                }

                newList.Add(newFileName);
            }

            return newList;
        }

        private static List<string> GetAllFilesFromDirectory(string directoryName, params string[] filePatterns)
        {
            var filesList = new List<string>();
            foreach (string filePattern in filePatterns)
            {
                string[] files = Directory.GetFiles(directoryName, filePattern, SearchOption.AllDirectories);
                filesList.AddRange(files);
            }

            return filesList;
        }

        #endregion Public Methods and Operators
    }
}
