﻿using System;
using System.Linq;
using System.Text;

namespace FAES.AES.Compatability
{
    internal class MetaDataFAES
    {
        protected byte[] _passwordHint, _encryptionTimestamp, _encryptionVersion, _compression;

        /// <summary>
        /// Converts FAESv2 MetaData into easy-to-manage method calls
        /// </summary>
        /// <param name="metaData">Raw FAESv2 MetaData</param>
        public MetaDataFAES(byte[] metaData)
        {
            if (metaData != null)
            {
                try
                {
                    _passwordHint = metaData.Take(64).ToArray();
                    _encryptionTimestamp = metaData.Skip(64).Take(4).ToArray();
                    _encryptionVersion = metaData.Skip(68).Take(16).ToArray();
                    _compression = metaData.Skip(84).Take(6).ToArray();
                }
                catch (Exception e)
                {
                    string msg = "MetaData (FAESv2) was shorter than expected! This probably means you are decrypting an older file; If so, this isnt a problem. If not, something is wrong.";

                    if (FileAES_Utilities.GetVerboseLogging())
                        Logging.Log(String.Format("{0} | {1}", msg, e.ToString()), Severity.WARN);
                    else
                        Logging.Log(msg, Severity.WARN);
                }
            }
        }

        /// <summary>
        /// Gets the Password Hint
        /// </summary>
        /// <returns>Password Hint stored in MetaData</returns>
        public string GetPasswordHint()
        {
            if (_passwordHint != null)
                return ConvertBytesToString(_passwordHint).TrimEnd('\n', '\r', '¬', '�'); //Removes the old padding character used in older FAES versions, as well as any newlines or special chars
            else
                return "No Password Hint Set";
        }

        /// <summary>
        /// Gets the UNIX timestamp of when the file was encrypted (UTC time)
        /// </summary>
        /// <returns>UNIX timestamp (UTC)</returns>
        public int GetEncryptionTimestamp()
        {
            if (_encryptionTimestamp != null)
                return BitConverter.ToInt32(_encryptionTimestamp, 0);
            else
                return -1;
        }

        /// <summary>
        /// Gets the Version of FAES used to encrypt the file
        /// </summary>
        /// <returns>FAES Version</returns>
        public string GetEncryptionVersion()
        {
            if (_encryptionVersion != null)
            {
                string ver = ConvertBytesToString(_encryptionVersion);

                if (ver.Contains("DEV"))
                    return ver.Split('_')[0];
                else
                    return ver;
            }
            else
                return "v1.1.0 — v1.1.2";
        }

        /// <summary>
        /// Gets the Compression Method used to compress the encrypted file
        /// </summary>
        /// <returns>Compression Mode Type</returns>
        public string GetCompressionMode()
        {
            if (_compression != null)
            {
                string converted = ConvertBytesToString(_compression);

                if (!String.IsNullOrEmpty(converted))
                    return converted;
            }
            return "LGYZIP";
        }

        /// <summary>
        /// Gets raw metadata
        /// </summary>
        /// <returns>MetaData byte array (256 bytes)</returns>
        public byte[] GetMetaData()
        {
            byte[] formedMetaData = new byte[256];
            Buffer.BlockCopy(_passwordHint, 0, formedMetaData, 0, _passwordHint.Length);
            Buffer.BlockCopy(_encryptionTimestamp, 0, formedMetaData, 64, _encryptionTimestamp.Length);
            Buffer.BlockCopy(_encryptionVersion, 0, formedMetaData, 68, _encryptionVersion.Length);
            Buffer.BlockCopy(_compression, 0, formedMetaData, 84, _compression.Length);

            return formedMetaData;
        }

        /// <summary>
        /// Converts a string into a padded/truncated byte array
        /// </summary>
        /// <param name="value">String to convert</param>
        /// <param name="maxLength">Max length of the string</param>
        /// <param name="paddingChar">Character used to pad the string if required</param>
        /// <returns>Byte Array</returns>
        private byte[] ConvertStringToBytes(string value, int maxLength = 64)
        {
            if (value.Length > maxLength)
                return Encoding.UTF8.GetBytes(value.Substring(0, maxLength));
            else
                return Encoding.UTF8.GetBytes(value.PadRight(maxLength, '\0'));
        }

        /// <summary>
        /// Converts a byte array to a string, ensuring all NULL values are trimmed
        /// </summary>
        /// <param name="value">Byte Array</param>
        /// <returns>String</returns>
        private string ConvertBytesToString(byte[] value)
        {
            return Encoding.UTF8.GetString(value).TrimEnd('\0', '\n', '\r', '¬', '�'); //Removes the old padding character used in older FAES versions
        }
    }
}
