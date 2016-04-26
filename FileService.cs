using Amazon.S3;
using Amazon.S3.Model;
using Glimpse.AspNet.Tab;
using MoGoMe.Data;
using MoGoMe.Web.Controllers.Api;
using MoGoMe.Web.Core;
using MoGoMe.Web.Domain;
using MoGoMe.Web.Models;
using MoGoMe.Web.Models.Requests;
using MoGoMe.Web.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;

namespace MoGoMe.Web.Services
{
    public class FileService : BaseService, IFileService
    {


        private ISiteConfig _config;

        public FileService(ISiteConfig config)
        {
            this._config = config;
        }

        public int Insert(FileAddRequest model, string userId, string clientId)
        {
            //Guid uid = Guid.Empty;//000-0000-0000-0000
            int Id = 0;

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Files_Insert"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@FileType", model.FileType);
                   paramCollection.AddWithValue("@FriendlyName", model.FriendlyName);
                   paramCollection.AddWithValue("@FilePath", model.FilePath);
                   paramCollection.AddWithValue("@UserId", userId);
                   paramCollection.AddWithValue("@ClientId", clientId);

                   SqlParameter p = new SqlParameter("@Id", System.Data.SqlDbType.Int);
                   p.Direction = System.Data.ParameterDirection.Output;

                   paramCollection.Add(p);

               },
               returnParameters: delegate (SqlParameterCollection param)
               {
                   int.TryParse(param["@Id"].Value.ToString(), out Id);
               }
               );

            return Id;
        }

        public void Update(FileUpdateRequest model, string userId)
        {
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Files_Update"
           , inputParamMapper: delegate (SqlParameterCollection paramCollection)
           {
               paramCollection.AddWithValue("@FileType", model.FileType);
               paramCollection.AddWithValue("@FriendlyName", model.FriendlyName);
               paramCollection.AddWithValue("@FilePath", model.FilePath);
               paramCollection.AddWithValue("@UserId", userId);
               paramCollection.AddWithValue("@Id", model.Id);

           });
        }

        public List<File> GetFiles()
        {
            List<File> list = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.Files_SelectAll"
               , inputParamMapper: null
               , map: delegate (IDataReader reader, short set)
               {
                   var f = new File();
                   int startingIndex = 0;

                   f.Id = reader.GetSafeInt32(startingIndex++);
                   f.FileType = reader.GetSafeInt16(startingIndex++);
                   f.FriendlyName = reader.GetSafeString(startingIndex++);
                   f.FilePath = reader.GetSafeString(startingIndex++);
                   f.UserId = reader.GetSafeString(startingIndex++);
                   f.DateAdded = reader.GetSafeDateTime(startingIndex++);
                   f.DateModified = reader.GetSafeDateTime(startingIndex++);

                   if (list == null)
                   {
                       list = new List<File>();
                   }

                   list.Add(f);
               }
               );
            return list;
        }

        public File Get(int Id)
        {
            File item = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.Files_SelectById"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@Id", Id);
                   //model binding
               }, map: delegate (IDataReader reader, short set)
               {
                   item = new File();
                   int startingIndex = 0; //startingOrdinal

                   item.Id = reader.GetSafeInt32(startingIndex++);
                   item.FileType = reader.GetSafeInt16(startingIndex++);
                   item.FriendlyName = reader.GetSafeString(startingIndex++);
                   item.FilePath = reader.GetSafeString(startingIndex++);
                   item.UserId = reader.GetSafeString(startingIndex++);
                   item.DateAdded = reader.GetSafeDateTime(startingIndex++);
                   item.DateModified = reader.GetSafeDateTime(startingIndex++);
               }
               );
            return item;
        }

        public List<File> GetByClientId(string clientId)
        {
            List<File> list = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.Files_SelectByClientId"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@ClientId", clientId);
               }
               , map: delegate (IDataReader reader, short set)

               {
                   var f = new File();
                   int startingIndex = 0;

                   f.Id = reader.GetSafeInt32(startingIndex++);
                   f.FileType = reader.GetSafeInt16(startingIndex++);
                   f.FriendlyName = reader.GetSafeString(startingIndex++);
                   f.FilePath = reader.GetSafeString(startingIndex++);
                   f.UserId = reader.GetSafeString(startingIndex++);

                   if (list == null)
                   {
                       list = new List<File>();
                   }

                   list.Add(f);
               }
               );
            return list;
        }

        public void Delete(FileDelete model)
        {
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Files_Delete"
           , inputParamMapper: delegate (SqlParameterCollection paramCollection)
           {

               paramCollection.AddWithValue("@Id", model.Id);

           });
        }

        public string Root()
        {
            return "Root";
        }

        public BaseFile Upload(HttpPostedFile file, Int16 fileType, string userId, int accountId, string clientId)
        {
            PutObjectResponse response;
            Guid uid = Guid.NewGuid();
            string uniqueName = uid.ToString() + file.FileName;
            string resultingFileName = null;
            BaseFile baseFile = new BaseFile();
            string folder = "Root/" + accountId + "/" + clientId;
            CreateFolder(_config.AWSBucketName, folder);

            IAmazonS3 client;
            using (client = new AmazonS3Client(_config.AwsAccessKey, _config.AwsSecretAccessKey, Amazon.RegionEndpoint.USWest2))
            {

                PutObjectRequest request = new PutObjectRequest()
                {
                    BucketName = _config.AWSBucketName,
                    CannedACL = S3CannedACL.PublicRead,//PERMISSION TO FILE PUBLIC ACCESIBLE
                    Key = folder + "/" + uniqueName,
                    InputStream = file.InputStream//SEND THE FILE STREAM
                };

                response = client.PutObject(request);

                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    resultingFileName = "/" + request.Key;

                    FileAddRequest model = new FileAddRequest();
                    model.FileType = fileType;
                    model.FriendlyName = file.FileName;
                    model.FilePath = resultingFileName;

                    baseFile.Id = Insert(model, userId, clientId);
                    baseFile.FilePath = resultingFileName;
                }
                else
                {
                    baseFile.Id = -1;
                }

            }

            return baseFile;
        }

        public void CreateFolder(string bucketName, string folderName)
        {
            IAmazonS3 client;
            using (client = new AmazonS3Client(_config.AwsAccessKey, _config.AwsSecretAccessKey, Amazon.RegionEndpoint.USWest2))
            {
                PutObjectResponse response;
                var folderKey = folderName + "/"; //end the folder name with "/"

                var request = new PutObjectRequest();

                request.BucketName = bucketName;

                request.StorageClass = S3StorageClass.Standard;
                request.ServerSideEncryptionMethod = ServerSideEncryptionMethod.None;

                request.Key = folderKey;

                request.ContentBody = string.Empty;

                response = client.PutObject(request);
            }

        }

        public bool DeleteFromServer(string FilePath)
        {
            bool isFileDeleted = false;
            DeleteObjectResponse response;
            IAmazonS3 client;
            using (client = new AmazonS3Client(_config.AwsAccessKey, _config.AwsSecretAccessKey, Amazon.RegionEndpoint.USWest2))
            {

                DeleteObjectRequest request = new DeleteObjectRequest()
                {
                    BucketName = _config.AWSBucketName,
                    Key = FilePath

                };

                response = client.DeleteObject(request);
                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    isFileDeleted = true;
                }

            }

            return isFileDeleted;
        }

    }
}