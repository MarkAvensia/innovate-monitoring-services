using System;
using System.Collections.Generic;

namespace NitroConnector.Models
{
    public class ConnectorStateDTO
    {
        public DateTime ChangeDateUtc { get; set; }
        public bool ShouldBePersisted { get; set; }
        public bool FileWasPotentiallyChanged { get; set; }
        public int EntityId { get; set; }
        public object Priority { get; set; }
        public Data Data { get; set; }
        public int FailedAttempts { get; set; }
        public OutboundLinksPerChannel OutboundLinksPerChannel { get; set; }
    }
    public class Data
    {
        public DataState DataState { get; set; }
        public List<int> Channels { get; set; }
        public List<object> RemovedFromChannels { get; set; }
    }
    public class OutboundLinksPerChannel
    {
    }
    public class DataState
    {
        public Entity Entity { get; set; }
        public object FileId { get; set; }
    }
    public class Entity
    {
        public int EntityId { get; set; }
        public string EntityTypeId { get; set; }
        public string DisplayName { get; set; }
        public Fields Fields { get; set; }
        public List<object> SpecificationFields { get; set; }
        public object LanguageResolver { get; set; }
    }
    public class Fields
    {
        public string ArticleAltEan { get; set; }
        public List<string> ArticleAreaOfUse { get; set; }
        public string ArticleAssortmentCode { get; set; }
        public List<object> ArticleBadge { get; set; }
        public object ArticleBase { get; set; }
        public List<object> ArticleBaseLayer { get; set; }
        public string ArticleBrand { get; set; }
        public List<object> ArticleBrandUsp { get; set; }
        public List<string> ArticleCampaignCategories { get; set; }
        public List<string> ArticleCategory { get; set; }
        public List<object> ArticleCommercialColor { get; set; }
        public object ArticleCommercialName { get; set; }
        public object ArticleDeliverytimeDate { get; set; }
        public int ArticleDeliverytimeDays { get; set; }
        public object ArticleDepthMax { get; set; }
        public object ArticleDepthMin { get; set; }
        public ArticleDescription ArticleDescription { get; set; }
        public List<object> ArticleDesign { get; set; }
        public object ArticleDiameterMax { get; set; }
        public object ArticleDiameterMin { get; set; }
        public object ArticleDiscontinuedFrom { get; set; }
        public string ArticleEan { get; set; }
        public object ArticleEffectMin { get; set; }
        public string ArticleEnrichmentStatus { get; set; }
        public List<string> ArticleERPMainCategory { get; set; }
        public List<string> ArticleERPSubCategory { get; set; }
        public object ArticleFeatureBullets { get; set; }
        public List<object> ArticleFinish { get; set; }
        public bool ArticleForceInstallerChoice { get; set; }
        public bool ArticleFSC { get; set; }
        public List<string> ArticleFunctions { get; set; }
        public object ArticleHeading { get; set; }
        public object ArticleHeightMax { get; set; }
        public object ArticleHeightMin { get; set; }
        public object ArticleHideFrom { get; set; }
        public object ArticleHideUntil { get; set; }
        public string ArticleId { get; set; }
        public string ArticleIncrements { get; set; }
        public List<object> ArticleInstallation { get; set; }
        public string ArticleInternalName { get; set; }
        public object ArticleIpCode { get; set; }
        public object ArticleLayerThicknessMax { get; set; }
        public object ArticleLayerThicknessMin { get; set; }
        public object ArticleLengthMax { get; set; }
        public object ArticleLengthMin { get; set; }
        public double ArticleListPriceNOK { get; set; }
        public List<string> ArticleMaterial { get; set; }
        public object ArticleMountingDistanceMax { get; set; }
        public object ArticleMountingDistanceMin { get; set; }
        public List<object> ArticleProperties { get; set; }
        public object ArticleShortDescription { get; set; }
        public object ArticleSuitableFor { get; set; }
        public string ArticleSupplier { get; set; }
        public string ArticleSupplierArticleNumber { get; set; }
        public string ArticleTag { get; set; }
        public object ArticleTechnicalDescription { get; set; }
        public object ArticleTexture { get; set; }
        public object ArticleThicknessMax { get; set; }
        public object ArticleThicknessMin { get; set; }
        public object ArticleTouch { get; set; }
        public List<string> ArticleType { get; set; }
        public string ArticleUnitType { get; set; }
        public double ArticleVolume { get; set; }
        public double ArticleWeight { get; set; }
        public object ArticleWidthMax { get; set; }
        public object ArticleWidthMin { get; set; }
        public object AtricleChemicals { get; set; }
        public object AtricleEffectMax { get; set; }
    }
    public class ArticleDescription
    {
        public string en { get; set; }
        public string no { get; set; }
    }

    

   






}
