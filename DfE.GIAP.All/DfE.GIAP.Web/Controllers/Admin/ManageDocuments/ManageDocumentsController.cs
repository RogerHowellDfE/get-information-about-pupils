using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Constants.DsiConfiguration;
using DfE.GIAP.Common.Constants.Messages.Articles;
using DfE.GIAP.Common.Constants.Messages.Common;
using DfE.GIAP.Common.Constants.Routes;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Core.Models;
using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Core.Models.Editor;
using DfE.GIAP.Core.Models.News;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Service.ManageDocument;
using DfE.GIAP.Service.News;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.ViewModels;
using DfE.GIAP.Web.ViewModels.Admin;
using DfE.GIAP.Web.ViewModels.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace DfE.GIAP.Web.Controllers.Admin.ManageDocuments
{
    [Route(ApplicationRoute.Admin)]
    [Authorize(Roles = Role.Admin)]
    public class ManageDocumentsController : Controller
    {
        private readonly IManageDocumentsService _manageDocumentsService;
        private readonly IContentService _contentService;
        private readonly INewsService _newsService;

        public ManageDocumentsController(INewsService newsService, IManageDocumentsService manageDocumentsService, IContentService contentService)
        {
            _newsService = newsService ??
                throw new ArgumentNullException(nameof(newsService));
            _manageDocumentsService = manageDocumentsService ??
                throw new ArgumentNullException(nameof(manageDocumentsService));
            _contentService = contentService ??
                throw new ArgumentNullException(nameof(contentService));
        }

        [HttpGet]
        [Route(ManageDocument.ManageDocuments)]
        public async Task<IActionResult> ManageDocuments(string docType, string docAction, string newsArticleId)
        {
            LoadDocumentsList();
            ViewBag.DisplayEditor = false;
            var model = new ManageDocumentsViewModel();

            var isValidUrlAction = CheckValidUrlAction(docAction);
            var isValidUrlDocType = CheckValidUrlDocType(docType);

            if (isValidUrlAction && isValidUrlDocType)
            {
                ViewBag.DisplayEditor = isValidUrlDocType;

                Enum.TryParse(docAction, out EditorActions action);

                switch (action)
                {
                    case EditorActions.add:
                        return View("../Admin/ManageDocuments/CreateNewsArticle", model);

                    case EditorActions.edit:
                        if (docType == DocumentType.Article.ToString())
                        {
                            await LoadNewsList().ConfigureAwait(false);
                            model.DocumentList = new Document { DocumentId = DocumentType.Article.ToString(), DocumentName = DocumentType.Article.ToString() };
                            model.SelectedNewsId = newsArticleId;
                            model.DocumentData = await GetSelectedNewsDocumentData(newsArticleId).ConfigureAwait(false);
                        }
                        else
                        {
                            model.DocumentList = new Document { DocumentId = docType };
                            model.DocumentData = await GetSelectedDocumentData(docType).ConfigureAwait(false);
                        }
                        return View("../Admin/ManageDocuments/ManageDocuments", model);

                    default:
                        break;
                }
            }
            return View("../Admin/ManageDocuments/ManageDocuments", model);
        }

        [HttpPost]
        [Route(ManageDocument.ManageDocuments)]
        public async Task<IActionResult> ManageDocuments(ManageDocumentsViewModel manageDocumentsModel, string discard, string edit)
        {
            LoadDocumentsList();

            if (manageDocumentsModel.DocumentList != null)
            {
                if (manageDocumentsModel.DocumentList.DocumentId == null)
                {
                    ModelState.AddModelError("Document.Id", CommonErrorMessages.AdminDocumentRequired);
                    manageDocumentsModel.HasInvalidDocumentList = true;
                }
                else
                {
                    if (manageDocumentsModel.DocumentList.DocumentId != DocumentType.Article.ToString()
                    || manageDocumentsModel.DocumentList.DocumentId == DocumentType.ArchivedNews.ToString())
                    {
                        manageDocumentsModel.SelectedNewsId = string.Empty;
                        manageDocumentsModel.ArchivedNewsId = string.Empty;
                    }

                    if (manageDocumentsModel.DocumentList.DocumentId == DocumentType.Article.ToString() &&
                                string.IsNullOrEmpty(manageDocumentsModel.SelectedNewsId))
                    {
                        await LoadNewsList().ConfigureAwait(false);
                    }
                    else if (manageDocumentsModel.DocumentList.DocumentId == DocumentType.ArchivedNews.ToString() &&
                                string.IsNullOrEmpty(manageDocumentsModel.ArchivedNewsId))
                    {
                        await LoadArchivedNewsList().ConfigureAwait(false);
                    }
                    else
                    {
                        ViewBag.DisplayEditor = true;
                        if (!string.IsNullOrEmpty(manageDocumentsModel.SelectedNewsId))
                        {
                            await LoadNewsList().ConfigureAwait(false);
                        }
                        else if (!string.IsNullOrEmpty(manageDocumentsModel.ArchivedNewsId))
                        {
                            await LoadArchivedNewsList().ConfigureAwait(false);
                        }

                        if (string.IsNullOrEmpty(edit))
                        {
                            manageDocumentsModel.DocumentData = !string.IsNullOrEmpty(manageDocumentsModel.SelectedNewsId) ?
                                                                    await GetSelectedNewsDocumentData(manageDocumentsModel.SelectedNewsId) :
                                                                    await GetSelectedDocumentData(manageDocumentsModel.DocumentList.DocumentId);
                            ModelState.Clear();
                        }
                    }
                }
            }
            else if (!string.IsNullOrEmpty(discard))
            {
                ViewBag.DisplayEditor = false;
                ModelState.Clear();
            }

            return View("../Admin/ManageDocuments/ManageDocuments", manageDocumentsModel);
        }

        [HttpPost]
        public async Task<IActionResult> SelectNewsArticle(ManageDocumentsViewModel manageDocumentsModel)
        {
            if (string.IsNullOrEmpty(manageDocumentsModel.SelectedNewsId))
            {
                manageDocumentsModel.HasInvalidNewsList = true;
                ModelState.AddModelError("SelectNewsArticle", CommonErrorMessages.AdminNewsArticleRequired);
            }
            return await ManageDocuments(manageDocumentsModel, null, null).ConfigureAwait(false);
        }

        #region AddNewsArticle

        [HttpPost]
        [Route(ManageDocument.ManageDocumentsNewsArticleAdd)]
        public IActionResult CreateNewsArticle()
        {
            var manageDocumentsModel = new ManageDocumentsViewModel();
            manageDocumentsModel.BackButton = new(true, "ManageDocuments", "ManageDocuments");

            return View("../Admin/ManageDocuments/CreateNewsArticle", manageDocumentsModel);
        }

        #endregion AddNewsArticle

        #region DeleteNewsArticle

        [HttpPost]
        [Route(ManageDocument.ManageDocumentsNewsArticleDelete)]
        public async Task<IActionResult> DeleteNews(ManageDocumentsViewModel manageDocumentsModel)
        {
            var result = await _newsService.DeleteNewsArticle(manageDocumentsModel.SelectedNewsId).ConfigureAwait(false);

            if (result != System.Net.HttpStatusCode.OK)
            {
                return await GenerateErrorView(ArticleErrorMessages.DeleteError).ConfigureAwait(false);
            }

            manageDocumentsModel.Confirmation = new Confirmation
            {
                Title = ArticleSuccessMessages.DeleteTitle,
                Body = ArticleSuccessMessages.DeleteBody,
                Text = string.Empty
            };

            return View("../Admin/ManageDocuments/Confirmation", manageDocumentsModel);
        }

        #endregion DeleteNewsArticle

        #region DeleteArchiveNewsArticle

        [HttpPost]
        [Route(ManageDocument.ManageDocumentsArchivedNewsArticleDelete)]
        public async Task<IActionResult> DeleteArchiveNews(ManageDocumentsViewModel manageDocumentsModel)
        {
            var result = await _newsService.DeleteNewsArticle(manageDocumentsModel.DocumentData.Id).ConfigureAwait(false);

            if (result != System.Net.HttpStatusCode.OK)
            {
                return await GenerateErrorView(ArticleErrorMessages.ArchivedDeleteError).ConfigureAwait(false);
            }

            manageDocumentsModel.Confirmation = new Confirmation
            {
                Title = ArticleSuccessMessages.ArchivedDeleteTitle,
                Body = ArticleSuccessMessages.ArchivedDeleteBody,
            };

            return View("../Admin/ManageDocuments/Confirmation", manageDocumentsModel);
        }

        #endregion DeleteArchiveNewsArticle

        #region ArchivedNewsArticle

        [HttpPost]
        [Route(ManageDocument.ManageDocumentsNewsArticleArchived)]
        public async Task<IActionResult> ArchivedNews(ManageDocumentsViewModel manageDocumentsModel)
        {
            if (string.IsNullOrEmpty(manageDocumentsModel.ArchivedNewsId))
            {
                manageDocumentsModel.HasInvalidArchiveList = true;
                ModelState.AddModelError("SelectArchivNewsArticle", CommonErrorMessages.AdminNewsArticleRequired);
                return await ManageDocuments(manageDocumentsModel, null, null).ConfigureAwait(false);
            }

            var newsItem = await GetSelectedNewsDocumentData(manageDocumentsModel.ArchivedNewsId).ConfigureAwait(false);
            manageDocumentsModel.DocumentData = newsItem;
            manageDocumentsModel.BackButton = new(true, "ManageDocuments", "ManageDocuments");

            return View("../Admin/ManageDocuments/ArchivedNews", manageDocumentsModel);
        }

        #endregion ArchivedNewsArticle

        #region ArchiveNewsArticle

        [HttpPost]
        [Route(ManageDocument.ManageDocumentsNewsArticleArchive)]
        public async Task<IActionResult> ArchiveNews(ManageDocumentsViewModel manageDocumentsModel)
        {
            var result = await UnarchiveOrArchiveNewsDocument(manageDocumentsModel, ActionTypes.Archive).ConfigureAwait(false);

            if (result is null)
            {
                return await GenerateErrorView(ArticleErrorMessages.ArchiveError).ConfigureAwait(false);
            }

            manageDocumentsModel.Confirmation = new Confirmation
            {
                Title = ArticleSuccessMessages.ArchiveTitle,
                Body = ArticleSuccessMessages.ArchiveBody,
            };

            return View("../Admin/ManageDocuments/Confirmation", manageDocumentsModel);
        }

        #endregion ArchiveNewsArticle

        #region UnarchiveNewsArticle

        [HttpPost]
        [Route(ManageDocument.ManageDocumentsNewsArticleUnarchive)]
        public async Task<IActionResult> UnarchiveNews(ManageDocumentsViewModel manageDocumentsModel)
        {
            var result = await UnarchiveOrArchiveNewsDocument(manageDocumentsModel, ActionTypes.Unarchive).ConfigureAwait(false);

            if (result is null)
            {
                return await GenerateErrorView(ArticleErrorMessages.UnarchiveError).ConfigureAwait(false);
            }

            manageDocumentsModel.Confirmation = new Confirmation
            {
                Title = ArticleSuccessMessages.UnarchiveTitle,
                Body = ArticleSuccessMessages.UnarchiveBody,
            };

            return View("../Admin/ManageDocuments/Confirmation", manageDocumentsModel);
        }

        #endregion UnarchiveNewsArticle

        #region EditNewsArticle

        // TODO: Check if this roue is used, and check if "EditNewsArticle" view is needed?
        [HttpPost]
        [Route(ManageDocument.ManageDocumentsNewsArticleEdit)]
        public async Task<IActionResult> EditNewsArticle(ManageDocumentsViewModel manageDocumentsModel, string edit)
        {
            if (manageDocumentsModel.DocumentData != null)
            {
                if (!string.IsNullOrEmpty(edit) && !string.IsNullOrEmpty(manageDocumentsModel.SelectedNewsId))
                {
                    manageDocumentsModel.DocumentData = await GetSelectedNewsDocumentData(manageDocumentsModel.SelectedNewsId);
                }
            }
            ModelState.Clear();
            return View("../Admin/ManageDocuments/EditNewsArticle", manageDocumentsModel);
        }

        #endregion EditNewsArticle

        #region PreviewNewsArticle

        [HttpPost]
        [Route(ManageDocument.ManageDocumentsNewsArticlePreview)]
        public async Task<IActionResult> PreviewNewsArticle(ManageDocumentsViewModel manageDocumentsModel, string create, string preview)
        {
            if (ModelState.IsValid)
            {
                if (manageDocumentsModel.DocumentData != null)
                {
                    var pinned = manageDocumentsModel.DocumentData.Pinned;
                    var getResult = await SaveNewsArticleAsDraft(manageDocumentsModel);

                    if (getResult is null)
                    {
                        return await GenerateErrorView(ArticleErrorMessages.SaveDraftError).ConfigureAwait(false);
                    }

                    manageDocumentsModel.DocumentData = getResult;
                    manageDocumentsModel.SelectedNewsId = getResult.Id;

                    manageDocumentsModel.DocumentData.Pinned = pinned;

                    var updatedResult = await SetPinned(manageDocumentsModel);

                    if (updatedResult is null)
                    {
                        return await GenerateErrorView(ArticleErrorMessages.UpdatedError).ConfigureAwait(false);
                    }
                }

                return View("../Admin/ManageDocuments/PreviewNewsArticle", manageDocumentsModel);
            }

            if (!string.IsNullOrEmpty(preview))
            {
                ModelState.Clear();
                return View("../Admin/ManageDocuments/EditNewsArticle", manageDocumentsModel);
            }
            else
            {
                return View("../Admin/ManageDocuments/CreateNewsArticle", manageDocumentsModel);
            }
        }

        #endregion PreviewNewsArticle

        #region PreviewManageDocument

        [HttpPost]
        [Route(ManageDocument.ManageDocumentsPreview)]
        public async Task<IActionResult> PreviewChanges(ManageDocumentsViewModel manageDocumentsModel, string preview)
        {
            if (ModelState.IsValid)
            {
                var updatedResult = await SetPinned(manageDocumentsModel);

                if (updatedResult is null)
                {
                    return await GenerateErrorView(ArticleErrorMessages.UpdatedError).ConfigureAwait(false);
                }

                ViewBag.DisplayEditor = true;
                return View("../Admin/ManageDocuments/PreviewChanges", manageDocumentsModel);
            }

            LoadDocumentsList();
            ViewBag.DisplayEditor = true;
            manageDocumentsModel.DocumentData = await GetSelectedDocumentData(manageDocumentsModel.DocumentList.DocumentId);

            return View("../Admin/ManageDocuments/ManageDocuments", manageDocumentsModel);
        }

        #endregion PreviewManageDocument

        #region PublishNewsArticle

        [HttpPost]
        [Route(ManageDocument.ManageDocumentsNewsArticlePublish)]
        public async Task<IActionResult> PublishNewsArticle(ManageDocumentsViewModel manageDocumentsModel, string publish)
        {
            if (manageDocumentsModel != null)
            {
                var result = await Publish(manageDocumentsModel);
                if (result is null)
                {
                    return await GenerateErrorView(ArticleErrorMessages.PublishError);
                }
            }

            manageDocumentsModel.Confirmation = new Confirmation
            {
                Title = ArticleSuccessMessages.PublishTitle,
                Body = ArticleSuccessMessages.PublishBody
            };

            return View("../Admin/ManageDocuments/Confirmation", manageDocumentsModel);
        }

        #endregion PublishNewsArticle

        #region SaveArticleAsDraft

        [HttpPost]
        [Route(ManageDocument.ManageDocumentsNewsArticleSaveAsDraft)]
        public async Task<IActionResult> SaveArticleAsDraft(ManageDocumentsViewModel manageDocumentsModel, string publish)
        {
            if (manageDocumentsModel != null)
            {
                var updatedResult = await SetPinned(manageDocumentsModel);

                if (updatedResult is null)
                {
                    return await GenerateErrorView(ArticleErrorMessages.UpdatedError);
                }

                var draftResult = await SaveNewsArticleAsDraft(manageDocumentsModel);

                if (draftResult is null)
                {
                    return await GenerateErrorView(ArticleErrorMessages.SaveDraftError);
                }
            }

            manageDocumentsModel.Confirmation = new Confirmation
            {
                Title = ArticleSuccessMessages.SaveAsDraftTitle,
                Body = ArticleSuccessMessages.SaveAsDraftBody
            };

            return View("../Admin/ManageDocuments/Confirmation", manageDocumentsModel);
        }

        #endregion SaveArticleAsDraft

        #region PublishManageDocument

        [HttpPost]
        [Route(ManageDocument.ManageDocumentsPublish)]
        public async Task<IActionResult> PublishChanges(ManageDocumentsViewModel manageDocumentsModel, string publish)
        {
            if (manageDocumentsModel != null)
            {
                var draftResult = await SaveDraft(manageDocumentsModel).ConfigureAwait(false);

                if (draftResult is null)
                {
                    var userErrorMessage = GenerateErrorMessage(manageDocumentsModel);
                    return await GenerateErrorView(userErrorMessage);
                }

                var publishedDocument = await Publish(manageDocumentsModel).ConfigureAwait(false);

                if (publishedDocument is null)
                {
                    var userErrorMessage = GenerateErrorMessage(manageDocumentsModel);
                    return await GenerateErrorView(userErrorMessage);
                }
            }

            manageDocumentsModel.Confirmation = GenerateConfirmationMessage(manageDocumentsModel);

            return View("../Admin/ManageDocuments/Confirmation", manageDocumentsModel);
        }

        #endregion PublishManageDocument

        #region PrivateMethods

        private async Task<CommonResponseBodyViewModel> SetPinned(ManageDocumentsViewModel manageDocumentsModel)
        {
            var isNewsArticle = !string.IsNullOrEmpty(manageDocumentsModel.SelectedNewsId);

            var responseToPublish = new CommonRequestBody
            {
                Title = manageDocumentsModel.DocumentData.Title,
                Body = manageDocumentsModel.DocumentData.Body,
                UserAccount = Global.UserAccount,
                Username = Global.UserName,
                Id = isNewsArticle ? manageDocumentsModel.SelectedNewsId : manageDocumentsModel.DocumentData.Id,
                DocType = isNewsArticle ? (int)UpdateDocumentType.NewsArticles : (int)UpdateDocumentType.Content,
                Published = true,
                Action = manageDocumentsModel.DocumentData.Pinned ? (int)ActionTypes.Pinned : (int)ActionTypes.Unpinned
            };

            var results = await _contentService.SetDocumentToPublished(responseToPublish, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())).ConfigureAwait(false);

            if (results is null) return default;

            return results.ConvertToViewModel();
        }

        private async Task<CommonResponseBodyViewModel> Publish(ManageDocumentsViewModel manageDocumentsModel)
        {
            var isNewsArticle = !string.IsNullOrEmpty(manageDocumentsModel.SelectedNewsId);

            var updatedResult = await SetPinned(manageDocumentsModel);
            if (updatedResult is null) return default;

            var responseToPublish = new CommonRequestBody
            {
                UserAccount = Global.UserAccount,
                Username = Global.UserName,
                Id = isNewsArticle ? manageDocumentsModel.SelectedNewsId : manageDocumentsModel.DocumentData.Id,
                DocType = isNewsArticle ? (int)UpdateDocumentType.NewsArticles : (int)UpdateDocumentType.Content,
                Published = true,
                Pinned = manageDocumentsModel.DocumentData.Pinned,
                Action = (int)ActionTypes.Publish
            };

            var results = await _contentService.SetDocumentToPublished(responseToPublish, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())).ConfigureAwait(false);

            if (results is null) return default;

            return results.ConvertToViewModel();
        }

        private async Task<CommonResponseBodyViewModel> SaveDraft(ManageDocumentsViewModel manageDocumentsModel)
        {
            var isNewsArticle = !string.IsNullOrEmpty(manageDocumentsModel.SelectedNewsId);

            var updatedResult = await SetPinned(manageDocumentsModel);
            if (updatedResult is null) return default;

            var responseToSave = new CommonRequestBody
            {
                Title = manageDocumentsModel.DocumentData.Title,
                Body = manageDocumentsModel.DocumentData.Body,
                Pinned = manageDocumentsModel.DocumentData.Pinned,
                UserAccount = Global.UserAccount,
                Username = Global.UserName,
                Id = isNewsArticle ? manageDocumentsModel.SelectedNewsId : manageDocumentsModel.DocumentData.Id,
                DocType = isNewsArticle ? (int)UpdateDocumentType.NewsArticles : (int)UpdateDocumentType.Content,
                Published = false,
            };

            var results = await _contentService.AddOrUpdateDocument(responseToSave, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())).ConfigureAwait(false);

            if (results is null) return default;

            return results.ConvertToViewModel();
        }

        private async Task<CommonResponseBodyViewModel> SaveNewsArticleAsDraft(ManageDocumentsViewModel manageDocumentsModel)
        {
            var isNewsArticle = !string.IsNullOrEmpty(manageDocumentsModel.SelectedNewsId);
            var isTitle = !string.IsNullOrEmpty(manageDocumentsModel.DocumentData.Title);

            var responseToSave = new CommonRequestBody
            {
                Title = isTitle ? SecurityHelper.SanitizeText(manageDocumentsModel.DocumentData.Title) : SecurityHelper.SanitizeText(manageDocumentsModel.DocumentData.DraftTitle),
                Body = isTitle ? SecurityHelper.SanitizeText(manageDocumentsModel.DocumentData.Body) : SecurityHelper.SanitizeText(manageDocumentsModel.DocumentData.DraftBody),
                Pinned = manageDocumentsModel.DocumentData.Pinned,
                UserAccount = Global.UserAccount,
                Username = Global.UserName,
                Id = isNewsArticle ? manageDocumentsModel.SelectedNewsId : string.Empty,
                DocType = (int)UpdateDocumentType.NewsArticles,
                Published = false,
            };

            var results = await _contentService.AddOrUpdateDocument(responseToSave, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())).ConfigureAwait(false);

            if (results is null) return default;

            return results.ConvertToViewModel();
        }

        private async Task<CommonResponseBodyViewModel> GetSelectedDocumentData(string documentName)
        {
            CommonResponseBodyViewModel commonResponseBodyViewModel = new CommonResponseBodyViewModel();

            Enum.TryParse(documentName, out DocumentType documentType);

            var commonResponseBody = await _contentService.GetContent(documentType);
            if (commonResponseBody != null)
            {
                commonResponseBodyViewModel = commonResponseBody.ConvertToViewModel();
            }

            return commonResponseBodyViewModel;
        }

        private async Task<CommonResponseBodyViewModel> GetSelectedNewsDocumentData(string newsArticleId)
        {
            CommonResponseBodyViewModel request = new CommonResponseBodyViewModel();
            var newsArticle = await _newsService.GetNewsArticleById(newsArticleId).ConfigureAwait(false);
            if (newsArticle != null)
            {
                request.Id = newsArticle.Id;
                request.Title = string.IsNullOrEmpty(newsArticle.DraftTitle) ? SecurityHelper.SanitizeText(newsArticle.Title) : SecurityHelper.SanitizeText(newsArticle.DraftTitle);
                request.Body = string.IsNullOrEmpty(newsArticle.DraftBody) ? SecurityHelper.SanitizeText(newsArticle.Body) : SecurityHelper.SanitizeText(newsArticle.DraftBody);
                request.UserAccount = newsArticle.UserAccount;
                request.Username = newsArticle.Username;
                request.Pinned = newsArticle.Pinned;
                request.Published = newsArticle.Published;
            }
            return request;
        }

        private async Task<CommonResponseBodyViewModel> UnarchiveOrArchiveNewsDocument(ManageDocumentsViewModel manageDocumentsModel, ActionTypes action)
        {
            var isNewsArticle = !string.IsNullOrEmpty(manageDocumentsModel.SelectedNewsId) || !string.IsNullOrEmpty(manageDocumentsModel.ArchivedNewsId);

            var responseToPublish = new CommonRequestBody
            {
                Title = manageDocumentsModel.DocumentData.Title,
                Body = manageDocumentsModel.DocumentData.Body,
                Pinned = manageDocumentsModel.DocumentData.Pinned,
                UserAccount = Global.UserAccount,
                Username = Global.UserName,
                Id = manageDocumentsModel.SelectedNewsId ?? manageDocumentsModel.DocumentData.Id,
                DocType = isNewsArticle ? (int)UpdateDocumentType.NewsArticles : (int)UpdateDocumentType.Content,
                Published = true,
                Action = (int)action
            };

            var results = await _contentService.SetDocumentToPublished(responseToPublish, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())).ConfigureAwait(false);

            if (results is null) return default;

            return results.ConvertToViewModel();
        }

        private void LoadDocumentsList()
        {
            var documentsList = _manageDocumentsService.GetDocumentsList();
            ViewBag.IsSuccess = documentsList.Count > 0 ? true : false;
            ViewBag.ListOfDocuments = new SelectList(documentsList, "DocumentId", "DocumentName");
        }

        private async Task LoadNewsList()
        {
            IList<Document> newsList = new List<Document>();
            var requestBody = new RequestBody() { ARCHIVED = false, DRAFTS = true };
            var result = await _newsService.GetNewsArticles(requestBody).ConfigureAwait(false);

            foreach (var news in result)
            {
                var status = news.Published ? "Published" : "Draft";
                var pinned = news.Pinned ? " | Pinned" : "";
                var date = news.ModifiedDate.ToString("dd/MM/yyyy", new CultureInfo("en-GB"));
                var name = string.IsNullOrEmpty(news.DraftTitle) ? news.Title : news.DraftTitle;

                newsList.Add(new Document
                {
                    DocumentName = $"{name} | {date} | {status} {pinned}",
                    DocumentId = news.Id,
                    IsEnabled = true
                });
            }

            ViewBag.IsSuccess = newsList.Count > 0 ? true : false;
            ViewBag.NewsDocuments = new SelectList(newsList, "DocumentId", "DocumentName");
        }

        private async Task LoadArchivedNewsList()
        {
            IList<Document> newsList = new List<Document>();
            var requestBody = new RequestBody() { ARCHIVED = true, DRAFTS = true };
            var result = await _newsService.GetNewsArticles(requestBody).ConfigureAwait(false);

            foreach (var news in result)
            {
                var status = news.Published ? "Published" : "Draft";
                var pinned = news.Pinned ? " | Pinned" : "";
                var date = news.ModifiedDate.ToString("dd/MM/yyyy", new CultureInfo("en-GB"));
                var name = string.IsNullOrEmpty(news.DraftTitle) ? news.Title : news.DraftTitle;

                newsList.Add(new Document
                {
                    DocumentName = $"{name} | {date} | {status} {pinned}",
                    DocumentId = news.Id,
                    IsEnabled = true
                });
            }

            ViewBag.ArchiveNewsIsSuccess = newsList.Count > 0 ? true : false;
            ViewBag.ArchivedNewsDocuments = new SelectList(newsList, "DocumentId", "DocumentName");
        }

        private Confirmation GenerateConfirmationMessage(ManageDocumentsViewModel model)
        {
            string title = string.Empty, body = string.Empty;

            if (!string.IsNullOrEmpty(model.SelectedNewsId) && model.DocumentData.Id != null)
            {
                title = ArticleSuccessMessages.UpdateTitle;
                body = ArticleSuccessMessages.UpdateBody;
            }
            else if (model.DocumentData.Id != null && string.IsNullOrEmpty(model.SelectedNewsId))
            {
                title = ArticleSuccessMessages.DocumentUpdatedTitle;
                body = ArticleSuccessMessages.DocumentUpdatedBody;
            }
            else if (model.DocumentData.Id == null && !string.IsNullOrEmpty(model.SelectedNewsId))
            {
                title = ArticleSuccessMessages.CreateTitle;
                body = ArticleSuccessMessages.CreateBody;
            }

            return new Confirmation
            {
                Title = title,
                Body = body
            };
        }

        private string GenerateErrorMessage(ManageDocumentsViewModel model)
        {
            string message = string.Empty;

            if (!string.IsNullOrEmpty(model.SelectedNewsId) && model.DocumentData.Id != null)
            {
                return ArticleErrorMessages.UpdatedError;
            }
            else if (model.DocumentData.Id != null && string.IsNullOrEmpty(model.SelectedNewsId))
            {
                return ArticleErrorMessages.UpdatedError;
            }
            else if (model.DocumentData.Id == null && string.IsNullOrEmpty(model.SelectedNewsId))
            {
                return ArticleErrorMessages.CreatedError;
            }

            return message;
        }

        private bool CheckValidUrlAction(string docAction)
        {
            if (!string.IsNullOrEmpty(docAction))
            {
                foreach (var e in Enum.GetValues(typeof(EditorActions)))
                {
                    var fieldInfo = e.GetType().GetField(e.ToString());
                    if (fieldInfo.Name.ToLower() == docAction.ToLower())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CheckValidUrlDocType(string urlDocType)
        {
            if (!string.IsNullOrEmpty(urlDocType))
            {
                foreach (var e in Enum.GetValues(typeof(DocumentType)))
                {
                    var fieldInfo = e.GetType().GetField(e.ToString());
                    if (fieldInfo.Name.ToLower() == urlDocType.ToLower())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private Task<ViewResult> GenerateErrorView(string userErrorMessage)
        {
            var userErrorModel = new UserErrorViewModel()
            {
                UserErrorMessage = userErrorMessage,
                BackButton = new(true, "ManageDocuments", "ManageDocuments")
            };

            return Task.FromResult(View("../Admin/ManageDocuments/Error", userErrorModel));
        }

        #endregion PrivateMethods
    }
}