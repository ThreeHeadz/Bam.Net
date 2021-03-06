/*
	Copyright © Bryan Apellanes 2015  
*/
// Model is Table
using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using Bam.Net;
using Bam.Net.Data;
using Bam.Net.Data.Qi;
using Bam.Net.Logging;
using Bam.Net.Configuration;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Engines;
using System.Linq;

namespace Bam.Net.Encryption
{
    /// <summary>
    /// An encrypted key value store used to prevent
    /// casual access to sensitive data like passwords.  Encrypted data is stored
    /// in a sqlite file by default or a Database you specify
    /// </summary>
	public partial class Vault
	{
        public Dictionary<string, string> ExportValues()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach(string key in Keys)
            {
                result.Add(key, this[key]);
            }
            return result;
        }

        public void ImportValues(Dictionary<string, string> values)
        {
            foreach(string key in values.Keys)
            {
                this[key] = values[key];
            }
        }

        public VaultKeyInfo ExportKey(Database db = null)
        {
            db = db ?? Database;
            VaultKey key = VaultKeysByVaultId.FirstOrDefault();
            _items = null;
            VaultKeysByVaultId.Delete(db);
            ChildCollections.Clear();
            VaultKeyInfo result = key.CopyAs<VaultKeyInfo>();
            return result;
        }

        public void ImportKey(VaultKeyInfo keyInfo, Database db = null)
        {
            VaultKey key = VaultKeysByVaultId.AddNew();
            key.RsaKey = keyInfo.RsaKey;
            key.Password = keyInfo.Password;
            key.Save(db);
            _items = null;
            ChildCollections.Clear();
        }


        static Database _defaultVaultDatabase;
        static object _defaultVaultDatabaseSync = new object();
        public static Database DefaultDatabase
        {
            get
            {
                return _defaultVaultDatabaseSync.DoubleCheckLock(ref _defaultVaultDatabase, () => InitializeDatabase());
            }
            set
            {
                _defaultVaultDatabase = value;
            }
        }

        internal static Database InitializeDatabase()
        {
            return InitializeDatabase(".\\System.vault.sqlite", Log.Default);
        }

        public static Database InitializeDatabase(string filePath, ILogger logger = null)
        {
            Database db = null;

            VaultDatabaseInitializer initializer = new VaultDatabaseInitializer(filePath);
            DatabaseInitializationResult result = initializer.Initialize();
            if (!result.Success)
            {
                logger.AddEntry(result.Exception.Message, result.Exception);
            }

            db = result.Database;

            return db;
        }

        protected static internal string Password
        {
            get
            {
                return "287802b5ca734821";
            }
        }

        static Vault _vault;
        static object _vaultSync = new object();
        public static Vault System
        {
            get
            {
                return _vaultSync.DoubleCheckLock(ref _vault, () =>
                {
                    return Retrieve(DefaultDatabase, "System", Password);
                });

            }
        }
        
        public static Vault Load(string filePath, string vaultName)
        {
            return Load(new FileInfo(filePath), vaultName);            
        }

        public static Vault Load(FileInfo file, string vaultName)
        {
            return Load(file, vaultName, "".RandomLetters(16)); // password will only be used if the file doesn't exist
        }

        static Dictionary<string, Vault> _loadedVaults = new Dictionary<string, Vault>();
        static object _loadedVaultsLock = new object();
        public static Vault Load(FileInfo file, string vaultName, string password, ILogger logger = null)
        {
            string key = $"{file.FullName}.{vaultName}";
            lock (_loadedVaultsLock)
            {
                if (!_loadedVaults.ContainsKey(key))
                {
                    if (logger == null)
                    {
                        logger = Log.Default;
                    }
                    Database db = InitializeDatabase(file.FullName, logger);
                    db.SelectStar = true;
                    _loadedVaults.Add(key, Retrieve(db, vaultName, password));
                }
            }
            return _loadedVaults[key];
        }

        /// <summary>
        /// Get the vault with the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Vault Retrieve(string name)
        {
            return Retrieve(DefaultDatabase, name, Secure.RandomString());
        }

        /// <summary>
        /// Get the Vault with the specified name using the
        /// specified password to create it if it doesn't exist
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static Vault Retrieve(string name, string password)
        {
            return Retrieve(DefaultDatabase, name, password);
        }

        /// <summary>
        /// Get a Vault from the specified database with the
        /// specified name using the specified password to
        /// create it if it doesn't exist.  Will return null
        /// if password is not specified and the vault 
        /// doesn't exist in the specified database
        /// </summary>
        /// <param name="database"></param>
        /// <param name="vaultName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static Vault Retrieve(Database database, string vaultName, string password = null)
        {
            Vault result = Vault.OneWhere(c => c.Name == vaultName, database);
            if (result == null && !string.IsNullOrEmpty(password))
            {
                result = Create(database, vaultName, password);
            }
            if (result != null)
            {
                result.Decrypt();
            }
            return result;
        }

        /// <summary>
        /// Create a vault in the specified file by the 
        /// specified name.  If the vault already exists
        /// in the specified file the existing vault
        /// will be returned
        /// </summary>
        /// <param name="file"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Vault Create(FileInfo file, string name)
        {
            string password = GeneratePassword();
            return Create(file, name, password);
        }

        public static Vault Create(FileInfo file, string name, string password)
        {
            Database db = InitializeDatabase(file.FullName, Log.Default);
            return Create(db, name, password);
        }

        public static Vault Create(string name)
        {
            string password = GeneratePassword();
            return Create(name, password);
        }

        public static string GeneratePassword()
        {
            SecureRandom random = new SecureRandom();
            string password = random.GenerateSeed(64).ToBase64();
            return password;
        }

        public static Vault Create(string name, string password)
        {
            Database db = InitializeDatabase();
            return Create(db, name, password);
        }

        /// <summary>
        /// Create a Vault in the specified database by the specified
        /// name using the specified password to create it if it
        /// doesn't exist
        /// </summary>
        /// <param name="database"></param>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <param name="rsaKeyLength"></param>
        /// <returns></returns>
        public static Vault Create(Database database, string name, string password, RsaKeyLength rsaKeyLength = RsaKeyLength._1024)
        {
            Vault result = Vault.OneWhere(c => c.Name == name, database);
            if (result == null)
            {
                result = new Vault();
                result.Name = name;
                result.Save(database);
                VaultKey key = result.VaultKeysByVaultId.JustOne(database, false);
                AsymmetricCipherKeyPair keys = RsaKeyGen.GenerateKeyPair(rsaKeyLength);
                key.RsaKey = keys.ToPem();
                key.Password = password.EncryptWithPublicKey(keys);
                key.Save(database);
            }

            return result;
        }

        public string ConnectionString
        {
            get
            {
                return Database.ConnectionString;
            }
        }

        Dictionary<string, DecryptedVaultItem> _items;
        object _itemsLock = new object();
        protected Dictionary<string, DecryptedVaultItem> Items
        {
            get
            {
                return _itemsLock.DoubleCheckLock(ref _items, () => new Dictionary<string, DecryptedVaultItem>());
            }
        }

        private bool Decrypt()
        {
            _items = null; // will cause it to reinitiailize above
            if(Key != null)
            {
                string password = Key.PrivateKeyDecrypt(Key.Password);
                VaultItemsByVaultId.Each(item =>
                {
                    string key = item.Key.AesPasswordDecrypt(password);
                    DecryptedVaultItem value = new DecryptedVaultItem(item, Key);
                    Items.Add(key, value);
                });
                return true;
            }
            return false;
        }
        
        protected VaultKey Key
        {
            get
            {
                return VaultKeysByVaultId.FirstOrDefault();
            }
        }

        public string[] Keys
        {
            get
            {
                Decrypt();
                string[] keys = new string[Items.Keys.Count];
                Items.Keys.CopyTo(keys, 0);
                return keys;
            }
        }

        public bool HasKey(string key)
        {
            string ignore;
            return HasKey(key, out ignore);
        }

        public bool HasKey(string key, out string value)
        {
            value = Get(key);
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Set a key value pair.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, string value)
        {
            this[key] = value;
        }

        public string Get(string key)
        {
            return this[key];
        }

        object writeLock = new object();
        public string this[string key]
        {
            get
            {
                if (Items.ContainsKey(key))
                {
                    return Items[key].Value;
                }
                else
                {
                    Decrypt();
                    if (Items.ContainsKey(key))
                    {
                        return Items[key].Value;
                    }
                }

                return null;
            }
            set
            {
                lock (writeLock)
                {
                    if (Decrypt())
                    {
                        if (Items.ContainsKey(key))
                        {
                            Items[key].Value = value;
                        }
                        else
                        {
                            VaultItem item = VaultItemsByVaultId.AddNew();
                            string password = Key.PrivateKeyDecrypt(Key.Password);
                            item.Key = key.AesPasswordEncrypt(password);
                            item.Value = value.AesPasswordEncrypt(password);
                            item.Save();
                            Items[key] = new DecryptedVaultItem(item, Key);
                        }
                    }
                }
            }
        }

        public Vault Copy(FileInfo file)
        {
            return Copy(file, Name);
        }

        public Vault Copy(FileInfo file, string name)
        {
            Vault copy = Vault.Load(file, name);
            Keys.Each(key =>
            {
                copy[key] = this[key];
            });

            return copy;
        }
	}
}																								
