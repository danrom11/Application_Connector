using System.IO.MemoryMappedFiles;

namespace Application_Connector
{
    public class Connector
    {
        /// <summary>
        /// Write-send data to memory application.
        /// </summary>
        /// <param name="data"> - Data to be transferred.</param>
        /// <param name="memoryName"> - Variable memory department name.</param>
        public static void Send(string data, string memoryName)
        {
            char[] message = data.ToCharArray();
            int size = message.Length;

            // Create a piece of shared memory
            // The first parameter is the name of the site,
            // the second is the length of the memory chunk in bytes: char type takes 2 bytes
            // plus four bytes for one Integer object

            MemoryMappedFile sharedMemory = MemoryMappedFile.CreateOrOpen(memoryName, size * 2 + 4);

            using (MemoryMappedViewAccessor writer = sharedMemory.CreateViewAccessor(0, size * 2 + 4))
            {
                // write to shared memory
                // write size from zero byte in shared memory
                writer.Write(0, size);
                // write message from the fourth byte in shared memory
                writer.WriteArray<char>(4, message, 0, message.Length);
            }
        }

        /// <summary>
        /// Accept-read data.
        /// </summary>
        /// <param name="memoryName"> - Variable memory department name.</param>
        public static string Accept(string memoryName)
        {
            // Array for message from shared memory
            char[] message;
            // The size of the entered message
            int size;

            // Get an existing piece of shared memory
            // Parameter - site name
            MemoryMappedFile sharedMemory = MemoryMappedFile.OpenExisting(memoryName);
            // First read the size of the message to create an array of the given size
            // Integer takes 4 bytes, starts from the first byte, so we transfer numbers 0 and 4
            using (MemoryMappedViewAccessor reader = sharedMemory.CreateViewAccessor(0, 4, MemoryMappedFileAccess.Read))
            {
                size = reader.ReadInt32(0);
            }

            // Read the message using the above size
            // The message is a string or array of char objects, each of which is two bytes
            // Therefore, as the second parameter, we pass the number of characters by multiplying by the size in bytes plus
            // And the first parameter is an offset - 4 bytes, which is the size of the message
            using (MemoryMappedViewAccessor reader = sharedMemory.CreateViewAccessor(4, size * 2, MemoryMappedFileAccess.Read))
            {
                //Массив символов сообщения
                message = new char[size];
                reader.ReadArray<char>(0, message, 0, size);
            }
            // Translation to string
            string strMessage = new string(message);
            return strMessage;
        }
    }
}
