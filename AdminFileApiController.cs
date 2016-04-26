using MoGoMe.Web.Models;
using MoGoMe.Web.Models.Requests;
using MoGoMe.Web.Models.Responses;
using MoGoMe.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MoGoMe.Web.Domain;
using System.Web;
using MoGoMe.Web.Core;
using Amazon.S3;
using System.IO;
using System.Net.Http.Headers;
using Amazon.S3.Model;

namespace MoGoMe.Web.Controllers.Api
{
    [RoutePrefix("api/admin/files")]
    public class FileApiController : AdminBaseApiController
    {

        IUserService _userService = null;
        IFileService _fileService = null;
        ISiteConfig _config = null;

        public FileApiController(IUserService userService, IFileService fileService, IClientStateService client, ISiteConfig config, IAdminUserService adminUserService) : base(client, adminUserService)
        {
            _userService = userService;
            _fileService = fileService;
            _config = config;

        }

        [Route, HttpPost]
        public HttpResponseMessage AddFile(FileAddRequest model)
        {
            
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            string userId = _userService.GetCurrentUserId();
            string clientId = ClientStateService.Read(ClientStateKey.CurrrentClientId, Request);

            ItemResponse<int> response = new ItemResponse<int>();
            response.Item = _fileService.Insert(model, userId, clientId);

            return Request.CreateResponse(response);
        }

        [Route("{id:int}"), HttpPut]
        public HttpResponseMessage Update(FileUpdateRequest model, int Id)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
            string userId = _userService.GetCurrentUserId();

            SuccessResponse response = new SuccessResponse();
            _fileService.Update(model, userId);
            return Request.CreateResponse(response);
        }

        [Route, HttpGet]
        public HttpResponseMessage GetByClientId()
        {
            string clientId = ClientStateService.Read(ClientStateKey.CurrrentClientId, Request);
            ItemsResponse<Domain.File> response = new ItemsResponse<Domain.File>();

            response.Items = _fileService.GetByClientId(clientId);
            return Request.CreateResponse(response);

        }

        [Route("{id:int}"), HttpGet]
        public HttpResponseMessage Get(int Id)
        {
            ItemResponse<Domain.File> response = new ItemResponse<Domain.File>();

            response.Item = _fileService.Get(Id);

            return Request.CreateResponse(response);

        }

        [Route("{id:int}"), HttpDelete]
        public HttpResponseMessage Delete(int Id)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            FileDelete model = new FileDelete();
            model.Id = Id;
            SuccessResponse response = new SuccessResponse();

            Domain.File fileToDelete = _fileService.Get(Id);
            String keyToDelete = fileToDelete.FilePath;

            _fileService.DeleteFromServer(keyToDelete);

            _fileService.Delete(model);


            return Request.CreateResponse(response);

        }

        [Route("{fileType:int}"), HttpPost]
        public HttpResponseMessage Upload(Int16 fileType)
        {
            HttpFileCollection files = HttpContext.Current.Request.Files;
            List<BaseFile> filesUploaded = null;
            bool allUploadsSucceeded = true;
            HttpStatusCode httpStatus = HttpStatusCode.OK;
            string userId = _userService.GetCurrentUserId();
            int accountId = _userService.GetAccountId(userId);
            string clientId = ClientStateService.Read(ClientStateKey.CurrrentClientId, Request);

            if (files == null || files.Count == 0)
            {

                return Request.CreateResponse(HttpStatusCode.BadRequest);

            }

            foreach (string fileName in files)
            {
                HttpPostedFile file = files[fileName];

                try
                {
                    BaseFile baseFile = _fileService.Upload(file, fileType, userId, accountId, clientId);

                    if (baseFile.FilePath == null)
                    {
                        string errorMessage = file.FileName + " failed to upload.";
                        baseFile.FilePath = errorMessage;

                        allUploadsSucceeded = false;

                        httpStatus = HttpStatusCode.InternalServerError;
                    }

                    if (filesUploaded == null)
                    {
                        filesUploaded = new List<BaseFile>();
                    }

                    filesUploaded.Add(baseFile);

                }
                catch (Exception e)
                {
                    allUploadsSucceeded = false;

                    httpStatus = HttpStatusCode.InternalServerError;

                    Request.CreateResponse(httpStatus, e.Message);
                }
            }
            ItemsResponse<BaseFile> uploadList = new ItemsResponse<BaseFile>();
            uploadList.IsSuccessFul = allUploadsSucceeded;

            uploadList.Items = filesUploaded;

            return Request.CreateResponse(httpStatus, uploadList);

        }

        [Route("link/{id:int}"), HttpGet]
        public HttpResponseMessage Download(int Id)
        {

            IAmazonS3 client;
            using (client = new AmazonS3Client(_config.AwsAccessKey, _config.AwsSecretAccessKey, Amazon.RegionEndpoint.USWest2))
            {
                var stream = new MemoryStream();

                Domain.File currentFile = _fileService.Get(Id);
                string awsKey = currentFile.FilePath;
                var slicedAwsKey = awsKey.Remove(0, 1);

                GetPreSignedUrlRequest request1 = new GetPreSignedUrlRequest()
                {
                    BucketName = _config.AWSBucketName,
                    Key = slicedAwsKey,
                    Expires = DateTime.Now.AddMinutes(1)
                };

                string url = client.GetPreSignedURL(request1);


                ItemResponse<string> response = new ItemResponse<string>();

                response.Item = url;

                return Request.CreateResponse(response);

            }
        }
    }
}

