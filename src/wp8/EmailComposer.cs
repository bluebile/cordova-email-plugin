/*
    Copyright 2013-2014 appPlant UG

    Licensed to the Apache Software Foundation (ASF) under one
    or more contributor license agreements.  See the NOTICE file
    distributed with this work for additional information
    regarding copyright ownership.  The ASF licenses this file
    to you under the Apache License, Version 2.0 (the
    "License"); you may not use this file except in compliance
    with the License.  You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing,
    software distributed under the License is distributed on an
    "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
    KIND, either express or implied.  See the License for the
    specific language governing permissions and limitations
    under the License.
*/

using De.Martinreinhardt.Cordova.Plugins.Email;
using Microsoft.Phone.Tasks;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Windows.ApplicationModel.Email;
using Windows.Storage;
using WPCordovaClassLib.Cordova;
using WPCordovaClassLib.Cordova.Commands;
using WPCordovaClassLib.Cordova.JSON;

namespace Cordova.Extension.Commands
{
    /// <summary>
    /// Implementes access to email composer task
    /// http://msdn.microsoft.com/en-us/library/windowsphone/develop/hh394003(v=vs.105).aspx
    /// </summary>
    public class EmailComposer : BaseCommand
    {
        /// <summary>
        /// Überprüft, ob Emails versendet werden können.
        /// </summary>
        public void isAvailable(string jsonArgs)
        {
            DispatchCommandResult(new PluginResult(PluginResult.Status.OK, true));
        }

        private async Task<StorageFile> GetTextFile(string fileContent)
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var file = await localFolder.CreateFileAsync("emailsharecontent.txt", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            //byte[] binaryData = WPCordovaClassLib.Cordova.JSON.JsonHelper.Deserialize<byte[]>(fileContent);
            await Windows.Storage.FileIO.WriteTextAsync(file, fileContent);
            return file;
        }

        /// <summary>
        /// Öffnet den Email-Kontroller mit vorausgefüllten Daten.
        /// </summary>
        public void open(string jsonArgs)
        {
            string[] args = JsonHelper.Deserialize<string[]>(jsonArgs);
            Options options = JsonHelper.Deserialize<Options>(args[0]);

            GetDraftWithProperties(options);
        }

        /// </summary>
        /// Erstellt den Email-Composer und fügt die übergebenen Eigenschaften ein.
        /// </summary>
        private async void GetDraftWithProperties(Options options)
        {
            try
            {
                EmailMessage email = new EmailMessage();
                email.Subject = options.Subject;
                email.Body = options.Body;

                for (int i = 0; i < options.Attachments.Length; i++) {
                    var file = await GetTextFile(options.Attachments[i]);
                    var fileName = "unnamed.file";

                    if (options.AttachmentsFileNames != null) {
                        if (options.AttachmentsFileNames[i] != null) {
                            fileName = options.AttachmentsFileNames[i];
                        }
                    }
                    
                    var att = new EmailAttachment(fileName, file);
                    email.Attachments.Add(att);
                }
                
                Deployment.Current.Dispatcher.BeginInvoke(async () => {
                    await EmailManager.ShowComposeNewEmailAsync(email);
                });

                DispatchCommandResult(new PluginResult(PluginResult.Status.OK, true));
            }
            catch (Exception ex)
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, ex.Message));
            }
        }
    }
}
