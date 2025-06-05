namespace DfE.GIAP.Common.Constants.Routes
{
    public static class ManageDocument
    {
        public const string ManageDocuments = "manage-documents/{docType?}/{docAction?}/{newsArticleId?}";
        public const string ManageDocumentsPreview = "manage-documents/preview";
        public const string ManageDocumentsPublish = "manage-documents/publish";
        public const string ManageDocumentsNewsArticleAdd = "manage-documents/article/add";
        public const string ManageDocumentsNewsArticleArchive = "manage-documents/article/archive";
        public const string ManageDocumentsNewsArticleArchived = "manage-documents/article/archived";
        public const string ManageDocumentsNewsArticleDelete = "manage-documents/article/delete";
        public const string ManageDocumentsArchivedNewsArticleDelete = "manage-documents/article/archived/delete";
        public const string ManageDocumentsNewsArticleEdit = "manage-documents/article/edit";
        public const string ManageDocumentsNewsArticlePreview = "manage-documents/article/preview";
        public const string ManageDocumentsNewsArticlePublish = "manage-documents/article/publish";
        public const string ManageDocumentsNewsArticleUnarchive = "manage-documents/article/unarchive";
        public const string ManageDocumentsNewsArticleSaveAsDraft = "manage-documents/article/save-as-draft";
    }
}
