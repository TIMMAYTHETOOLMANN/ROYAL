using System;
using System.IO;
using System.Text;

namespace BotCore.Utilities
{
    /// <summary>
    /// Encoding utilities to prevent BOM and encoding issues across the application
    /// </summary>
    public static class EncodingUtilities
    {
        /// <summary>
        /// UTF-8 encoding without BOM (Byte Order Mark)
        /// </summary>
        public static readonly UTF8Encoding UTF8WithoutBOM = new UTF8Encoding(false);

        /// <summary>
        /// Initialize encoding for the entire application
        /// </summary>
        public static void InitializeApplicationEncoding()
        {
            try
            {
                // Register code pages encoding provider for extended encoding support
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                // Set console encoding to UTF-8 without BOM
                Console.OutputEncoding = UTF8WithoutBOM;
                Console.InputEncoding = UTF8WithoutBOM;

                // Set environment variables for .NET runtime
                Environment.SetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "false");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not fully configure encoding: {ex.Message}");
            }
        }

        /// <summary>
        /// Read file with automatic BOM detection and removal
        /// </summary>
        public static string ReadFileWithoutBOM(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var bytes = File.ReadAllBytes(filePath);
            
            // Detect and skip BOM
            int startIndex = 0;
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            {
                // UTF-8 BOM detected
                startIndex = 3;
            }
            else if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            {
                // UTF-16 LE BOM detected
                return Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);
            }
            else if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            {
                // UTF-16 BE BOM detected
                return Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);
            }

            return UTF8WithoutBOM.GetString(bytes, startIndex, bytes.Length - startIndex);
        }

        /// <summary>
        /// Write file without BOM
        /// </summary>
        public static void WriteFileWithoutBOM(string filePath, string content)
        {
            File.WriteAllText(filePath, content, UTF8WithoutBOM);
        }

        /// <summary>
        /// Fix BOM issues in a file
        /// </summary>
        public static bool FixFileEncoding(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return false;
                }

                var content = ReadFileWithoutBOM(filePath);
                WriteFileWithoutBOM(filePath, content);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Fix BOM issues in all files in a directory
        /// </summary>
        public static int FixDirectoryEncoding(string directoryPath, string searchPattern = "*.json")
        {
            int fixedCount = 0;

            if (!Directory.Exists(directoryPath))
            {
                return fixedCount;
            }

            foreach (var file in Directory.GetFiles(directoryPath, searchPattern, SearchOption.AllDirectories))
            {
                if (FixFileEncoding(file))
                {
                    fixedCount++;
                }
            }

            return fixedCount;
        }

        /// <summary>
        /// Check if a file has BOM
        /// </summary>
        public static bool HasBOM(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            var bytes = File.ReadAllBytes(filePath);
            
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            {
                return true; // UTF-8 BOM
            }
            
            if (bytes.Length >= 2 && 
                ((bytes[0] == 0xFF && bytes[1] == 0xFE) || 
                 (bytes[0] == 0xFE && bytes[1] == 0xFF)))
            {
                return true; // UTF-16 BOM
            }

            return false;
        }

        /// <summary>
        /// Create StreamWriter without BOM
        /// </summary>
        public static StreamWriter CreateStreamWriterWithoutBOM(string filePath, bool append = false)
        {
            return new StreamWriter(filePath, append, UTF8WithoutBOM);
        }

        /// <summary>
        /// Create StreamReader with BOM detection
        /// </summary>
        public static StreamReader CreateStreamReaderWithBOMDetection(string filePath)
        {
            return new StreamReader(filePath, UTF8WithoutBOM, detectEncodingFromByteOrderMarks: true);
        }
    }
}

