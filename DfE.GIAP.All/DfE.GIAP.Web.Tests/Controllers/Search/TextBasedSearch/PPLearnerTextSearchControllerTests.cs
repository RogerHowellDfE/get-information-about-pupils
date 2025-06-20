﻿using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Constants.Messages.Downloads;
using DfE.GIAP.Common.Constants.Messages.Search;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Models.MPL;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Service.Common;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.MPL;
using DfE.GIAP.Service.Search;
using DfE.GIAP.Web.Controllers.TextBasedSearch;
using DfE.GIAP.Web.Helpers.Banner;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Tests.FakeData;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.Search.TextBasedSearch
{
    public class PPLearnerTextSearchControllerTests : IClassFixture<PaginatedResultsFake>, IClassFixture<SearchFiltersFakeData>
    {
        private readonly ILogger<PPLearnerTextSearchController> _mockLogger = Substitute.For<ILogger<PPLearnerTextSearchController>>();
        private readonly IDownloadService _mockDownloadService = Substitute.For<IDownloadService>();
        private readonly IPaginatedSearchService _mockPaginatedService = Substitute.For<IPaginatedSearchService>();
        private readonly IMyPupilListService _mockMplService = Substitute.For<IMyPupilListService>();
        private readonly ITextSearchSelectionManager _mockSelectionManager = Substitute.For<ITextSearchSelectionManager>();
        private readonly ICommonService _mockCommonService = Substitute.For<ICommonService>();
        private readonly IContentService _mockContentService = Substitute.For<IContentService>();
        private readonly IOptions<AzureAppSettings> _mockAppOptions = Substitute.For<IOptions<AzureAppSettings>>();
        private AzureAppSettings _mockAppSettings = new AzureAppSettings();
        private readonly ILatestNewsBanner _mockNewsBanner = Substitute.For<ILatestNewsBanner>();

        private readonly TestSession _mockSession = new TestSession();

        private readonly PaginatedResultsFake _paginatedResultsFake;
        private readonly SearchFiltersFakeData _searchFiltersFake;

        public PPLearnerTextSearchControllerTests(PaginatedResultsFake paginatedResultsFake, SearchFiltersFakeData searchFiltersFake)
        {
            _paginatedResultsFake = paginatedResultsFake;
            _searchFiltersFake = searchFiltersFake;
        }

        #region Search

        [Fact]
        public async Task NonUpnPupilPremiumDatabase_returns_empty_page_when_first_navigated_to()
        {
            // arrange
            SetupContentServicePublicationSchedule();

            // act
            var sut = GetController();
            var result = await sut.NonUpnPupilPremiumDatabase(null);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.NonUpnSearchView));

            Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerTextSearchViewModel;

            _mockSelectionManager.Received().Clear();
            AssertAbstractValues(sut, model);
            AssertContentServicePublicationValues(model);
            Assert.True(string.IsNullOrEmpty(model.SearchText));
        }

        [Fact]
        public async Task NonUpnPupilPremiumDatabase_clears_search_when_return_to_search_is_false()
        {
            // arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

            // act
            var sut = GetController();
            _mockSession.SetString(sut.SearchSessionKey, searchText);
            _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.NonUpnPupilPremiumDatabase(false);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.NonUpnSearchView));

            Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerTextSearchViewModel;

            AssertAbstractValues(sut, model);
            AssertContentServicePublicationValues(model);

            Assert.True(string.IsNullOrEmpty(model.SearchText));
            Assert.False(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        }

        [Fact]
        public async Task NonUpnPupilPremiumDatabase_return_to_search_page_persists_search()
        {
            // arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

            // act
            var sut = GetController();
            _mockSession.SetString(sut.SearchSessionKey, searchText);
            _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.NonUpnPupilPremiumDatabase(true);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.NonUpnSearchView));

            Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerTextSearchViewModel;

            AssertAbstractValues(sut, model);
            AssertContentServicePublicationValues(model);
            Assert.Equal(searchText, model.SearchText);
            Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        }

        [Fact]
        public async Task NonUpnPupilPremiumDatabase_does_not_call_GetPage_if_model_state_not_valid()
        {
            // arrange
            SetupContentServicePublicationSchedule();

            // act
            var sut = GetController();

            await sut.NonUpnPupilPremiumDatabase(new LearnerTextSearchViewModel(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

            // assert
            await _mockPaginatedService.DidNotReceive().GetPage(Arg.Any<string>(),
            Arg.Any<Dictionary<string, string[]>>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<AzureSearchIndexType>(),
            Arg.Any<AzureSearchQueryType>(),
            Arg.Any<AzureFunctionHeaderDetails>(),
            Arg.Any<string>(),
            Arg.Any<string>());
        }

        [Fact]
        public async Task NonUpnNationalPupilDatabase_populates_LearnerNumberIds_with_Id_when_UPN_0()
        {
            // arrange
            SetupContentServicePublicationSchedule();

            var sut = GetController();
            //override default user to make admin so Ids are not masked, not testing rbac rules for this test
            sut.ControllerContext.HttpContext.User = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            var response = new PaginatedResponse()
            {
                Learners = new List<Learner>()
                        {
                            new Learner()
                            {
                                Id = "123",
                                LearnerNumber = "0",
                            },
                            new Learner()
                            {
                                Id = "456",
                                LearnerNumber = "A203202811068",
                            }
                        },
                Count = 2
            };
            var expectedLearners = new List<Learner>()
                        {
                            new Learner()
                            {
                               Id = "123",
                                LearnerNumber = "0",
                                LearnerNumberId = "123",
                            },
                            new Learner()
                            {
                                Id = "456",
                                LearnerNumber = "A203202811068",
                                LearnerNumberId = "A203202811068",
                            }
            };

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, response);

            // act

            var result = await sut.NonUpnPupilPremiumDatabase(true);

            // assert

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);

            Assert.True(model.Learners.SequenceEqual(expectedLearners));
        }

        #region Search Filters

        [Theory]
        [ClassData(typeof(DobSearchFilterTestData))]
        public async Task DobFilter_Adds_DOB_month_and_year_filter_as_expected(SearchFilters searchFilter)
        {
            // Arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.DobFilter(searchViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var model = viewResult.Model as LearnerTextSearchViewModel;
            Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
            Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
            Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
            Assert.Equal(model.SearchFilters.CustomFilterText.DobDay, searchViewModel.SearchFilters.CustomFilterText.DobDay);
            Assert.Equal(model.SearchFilters.CustomFilterText.DobMonth, searchViewModel.SearchFilters.CustomFilterText.DobMonth);
            Assert.Equal(model.SearchFilters.CustomFilterText.DobYear, searchViewModel.SearchFilters.CustomFilterText.DobYear);
        }

        [Fact]
        public async Task DobFilter_returns_DobError_when_DobErrorEmpty()
        {
            // Arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var searchFilter = SetDobFilters(0, 0, 0);
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.DobFilter(searchViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var model = viewResult.Model as LearnerTextSearchViewModel;

            Assert.True(searchViewModel.FilterErrors.DobErrorEmpty);
            Assert.True(searchViewModel.FilterErrors.DobError);

            Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        }

        [Fact]
        public async Task DobFilter_returns_DobError_when_DobErrorDayOnly()
        {
            // Arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var searchFilter = SetDobFilters(1, 0, 0);
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.DobFilter(searchViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);

            Assert.True(searchViewModel.FilterErrors.DobErrorDayOnly);
            Assert.True(searchViewModel.FilterErrors.DobError);
        }

        [Fact]
        public async Task DobFilter_returns_DobError_when_DobErrorDayMonthOnly()
        {
            // Arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var searchFilter = SetDobFilters(1, 1, 0);
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.DobFilter(searchViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);

            Assert.True(searchViewModel.FilterErrors.DobErrorDayMonthOnly);
            Assert.True(searchViewModel.FilterErrors.DobError);
        }

        [Fact]
        public async Task DobFilter_returns_DobError_when_DayOutOfRange()
        {
            // Arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var searchFilter = SetDobFilters(99, 1, 2015);
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.DobFilter(searchViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);

            Assert.True(searchViewModel.FilterErrors.DayOutOfRange);
            Assert.True(searchViewModel.FilterErrors.DobError);
        }

        [Fact]
        public async Task DobFilter_returns_DobError_when_DobErrorMonthOnly()
        {
            // Arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var searchFilter = SetDobFilters(0, 1, 0);
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.DobFilter(searchViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);

            Assert.True(searchViewModel.FilterErrors.DobErrorMonthOnly);
            Assert.True(searchViewModel.FilterErrors.DobError);
        }

        [Fact]
        public async Task DobFilter_returns_DobError_when_DobErrorNoMonth()
        {
            // Arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var searchFilter = SetDobFilters(1, 0, 2015);
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.DobFilter(searchViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);

            Assert.True(searchViewModel.FilterErrors.DobErrorNoMonth);
            Assert.True(searchViewModel.FilterErrors.DobError);
        }

        [Fact]
        public async Task DobFilter_returns_DobError_when_MonthOutOfRange()
        {
            // Arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var searchFilter = SetDobFilters(1, 99, 2015);
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.DobFilter(searchViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);

            Assert.True(searchViewModel.FilterErrors.MonthOutOfRange);
            Assert.True(searchViewModel.FilterErrors.DobError);
        }

        [Fact]
        public async Task DobFilter_returns_DobError_when_YearLimitHigh()
        {
            // Arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var searchFilter = SetDobFilters(1, 2, 9999);
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.DobFilter(searchViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);

            Assert.True(searchViewModel.FilterErrors.YearLimitHigh);
            Assert.True(searchViewModel.FilterErrors.DobError);
        }

        [Fact]
        public async Task DobFilter_returns_DobError_when_YearLimitLow()
        {
            // Arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var searchFilter = SetDobFilters(1, 2, 1970);
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.DobFilter(searchViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);

            Assert.True(searchViewModel.FilterErrors.YearLimitLow);
            Assert.True(searchViewModel.FilterErrors.DobError);
        }

        [Fact]
        public async Task SurnameFilter_Returns_to_route_with_correct_surname_filter()
        {
            // Arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var surnameFilter = "Surname";
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.SurnameFilter(searchViewModel, surnameFilter);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var model = viewResult.Model as LearnerTextSearchViewModel;
            Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
            Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
            Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
            Assert.Equal(model.SearchFilters.CustomFilterText.Surname, searchViewModel.SearchFilters.CustomFilterText.Surname);
        }

        [Fact]
        public async Task MiddlenameFilter_Returns_to_route_with_correct_middlename_filter()
        {
            // Arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var middlenameFilter = "Middle";
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.MiddlenameFilter(searchViewModel, middlenameFilter);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var model = viewResult.Model as LearnerTextSearchViewModel;
            Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
            Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
            Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
            Assert.Equal(model.SearchFilters.CustomFilterText.Middlename, searchViewModel.SearchFilters.CustomFilterText.Middlename);
        }

        [Fact]
        public async Task ForneameFilter_Returns_to_route_with_correct_forename_filter()
        {
            // Arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var forenameFilter = "Forename";
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.ForenameFilter(searchViewModel, forenameFilter);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var model = viewResult.Model as LearnerTextSearchViewModel;
            Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
            Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
            Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
            Assert.Equal(model.SearchFilters.CustomFilterText.Forename, searchViewModel.SearchFilters.CustomFilterText.Forename);
        }

        [Theory]
        [InlineData("M")]
        [InlineData("F")]
        [InlineData("O")]
        public async Task GenderFilter_Returns_to_route_with_correct_gender_filter(string genderFilter)
        {
            // Arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters(), new string[] { genderFilter });

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.GenderFilter(searchViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var model = viewResult.Model as LearnerTextSearchViewModel;
            Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
            Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
            Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
            Assert.True(model.SelectedGenderValues[0].Equals(genderFilter));
        }

        [Fact]
        public async Task GenderFilter_returns_all_genders_when_no_gender_selected()
        {
            // Arrange
            SetupContentServicePublicationSchedule();
            var searchText = "Smith";
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters(), null);
            searchViewModel.SearchFilters.CurrentFiltersAppliedString = @"[{ ""FilterName"":""Female"",""FilterType"":6}]";

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.GenderFilter(searchViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var model = viewResult.Model as LearnerTextSearchViewModel;
            Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
            Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
            Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
            Assert.Null(model.SelectedGenderValues);
        }

        [Fact]
        public async Task GenderFilter_returns_all_genders_when_more_than_one_gender_deselected()
        {
            // Arrange
            SetupContentServicePublicationSchedule();
            var searchText = "Smith";
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters(), null);
            searchViewModel.SearchFilters.CurrentFiltersAppliedString = @"[{""FilterName"":""Female"",""FilterType"":6}, {""FilterName"":""Male"",""FilterType"":6}]";

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.GenderFilter(searchViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var model = viewResult.Model as LearnerTextSearchViewModel;
            Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
            Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
            Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
            Assert.Null(model.SelectedGenderValues);
        }

        #endregion Search Filters

        #endregion Search

        #region Add To My Pupil List

        [Fact]
        public async Task PPAddToMyPupilList_Adds_pupil_to_my_pupil_list_successfully()
        {
            // Arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var upn = _paginatedResultsFake.GetUpn();
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

            _mockSelectionManager.GetSelectedFromSession().Returns(upn);
            _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.PPAddToMyPupilList(searchViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var model = viewResult.Model as LearnerTextSearchViewModel;
            Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
            Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);

            await _mockMplService.Received().UpdateMyPupilList(
                Arg.Is<IEnumerable<MyPupilListItem>>(u => u.SequenceEqual(_paginatedResultsFake.GetUpnInMPL())),
                Arg.Any<string>(),
                Arg.Any<AzureFunctionHeaderDetails>()
                );
            Assert.True(model.ItemAddedToMyPupilList);
        }

        [Fact]
        public async Task PPAddToMyPupilList_Returns_to_search_page_if_no_pupil_selected()
        {
            // Arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var upn = _paginatedResultsFake.GetUpn();
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

            _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.AddToMyPupilList(searchViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var model = viewResult.Model as LearnerTextSearchViewModel;
            Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
            Assert.True(model.NoPupil);
            Assert.True(model.NoPupilSelected);
        }

        [Fact]
        public async Task PPAddToMyPupilList_redirects_to_InvalidUPNs_if_invalid_upn_selected()
        {
            // Arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var upn = _paginatedResultsFake.GetUpnsWithInvalid();
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

            _mockSelectionManager.GetSelectedFromSession().Returns(upn);
            _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Numbers, _paginatedResultsFake.GetInvalidLearners());
            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Id, new PaginatedResponse());

            var result = await sut.AddToMyPupilList(searchViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.IsType<InvalidLearnerNumberSearchViewModel>(viewResult.Model);
            Assert.Equal(Global.InvalidUPNsView, viewResult.ViewName);
        }

        [Fact]
        public async Task PPAddToMyPupilList_sets_download_links_correctly_on_serach_reload()
        {
            // Arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var upn = _paginatedResultsFake.GetUpn();
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

            _mockSelectionManager.GetSelectedFromSession().Returns(upn);
            _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.PPAddToMyPupilList(searchViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var model = viewResult.Model as LearnerTextSearchViewModel;
            Assert.Equal(ApplicationLabel.DownloadSelectedPupilPremiumDataLink, model.DownloadSelectedLink);
            Assert.Equal(ApplicationLabel.AddSelectedToMyPupilListLink, model.AddSelectedToMyPupilListLink);
        }

        #endregion Add To My Pupil List

        #region Download

        [Fact]
        public async Task ToDownloadSelectedPupilPremiumDataUPN_returns_data_when_pupil_selected()
        {
            // Arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var upn = _paginatedResultsFake.GetUpn();
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());
            searchViewModel.PageLearnerNumbers = upn;
            searchViewModel.SelectedPupil = upn;

            _mockSelectionManager.GetSelectedFromSession().Returns(upn);

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.ToDownloadSelectedPupilPremiumDataUPN(searchViewModel);

            // assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task DownloadPupilPremiumFile_downloads_file()
        {
            // arrange
            var upn = _paginatedResultsFake.GetUpn();
            var downloadViewModel = new LearnerDownloadViewModel
            {
                SelectedPupils = upn,
                LearnerNumber = upn,
                SelectedPupilsCount = 1,
                DownloadFileType = DownloadFileType.CSV,
                ShowTABDownloadType = true
            };
            downloadViewModel.TextSearchViewModel.StarredPupilConfirmationViewModel.ConfirmationGiven = true;

            _mockDownloadService.GetPupilPremiumCSVFile(
               Arg.Any<string[]>(),
               Arg.Any<string[]>(),
               true,
               Arg.Any<AzureFunctionHeaderDetails>(),
               Arg.Any<ReturnRoute>(),
               Arg.Any<UserOrganisation>())
               .Returns(new ReturnFile()
               {
                   FileName = "test",
                   FileType = "csv",
                   Bytes = new byte[0]
               });

            // act
            var sut = GetController();

            var result = await sut.DownloadPupilPremiumFile(downloadViewModel);

            // assert
            Assert.IsType<FileContentResult>(result);
        }

        [Fact]
        public async Task DownloadPupilPremiumFile_redirects_to_error_when_there_are_no_files_to_download()
        {
            // arrange
            var upn = _paginatedResultsFake.GetUpn();
            var downloadViewModel = new LearnerDownloadViewModel
            {
                SelectedPupils = upn,
                LearnerNumber = upn,
                SelectedPupilsCount = 1,
                DownloadFileType = DownloadFileType.CSV,
                ShowTABDownloadType = true
            };
            downloadViewModel.TextSearchViewModel.StarredPupilConfirmationViewModel.ConfirmationGiven = true;

            _mockDownloadService.GetPupilPremiumCSVFile(
               Arg.Any<string[]>(),
               Arg.Any<string[]>(),
               true,
               Arg.Any<AzureFunctionHeaderDetails>(),
               Arg.Any<ReturnRoute>(),
               Arg.Any<UserOrganisation>())
               .Returns(new ReturnFile()
               {
                   FileName = null,
                   FileType = null,
                   Bytes = null
               });

            // act
            var sut = GetController();

            var result = await sut.DownloadPupilPremiumFile(downloadViewModel);

            // assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task DownloadFileConfirmationReturn_downloads_file_when_confirmation_given()
        {
            // arrange
            var StarredPupilConfirmationViewModel = new StarredPupilConfirmationViewModel
            {
                SelectedPupil = _paginatedResultsFake.GetUpn(),
                DownloadType = DownloadType.PupilPremium,
                ConfirmationGiven = true,
                ConfirmationError = false,
                ConfirmationReturnAction = Global.PPDownloadConfirmationReturnAction,
                CancelReturnAction = Global.PPDownloadCancellationReturnAction,
                LearnerNumbers = _paginatedResultsFake.GetUpn()
            };

            _mockDownloadService.GetPupilPremiumCSVFile(
               Arg.Any<string[]>(),
               Arg.Any<string[]>(),
               Arg.Any<bool>(),
               Arg.Any<AzureFunctionHeaderDetails>(),
               Arg.Any<ReturnRoute>(),
               Arg.Any<UserOrganisation>())
               .Returns(new ReturnFile()
               {
                   FileName = "test",
                   FileType = "csv",
                   Bytes = new byte[0]
               });

            // act
            var sut = GetController();
            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());
            var result = await sut.DownloadFileConfirmationReturn(StarredPupilConfirmationViewModel);

            // assert
            Assert.IsType<FileContentResult>(result);
        }

        [Fact]
        public async Task DownloadFileConfirmationReturn_redirects_to_ConfirmationForStarredPupil_when_no_confirmation_given()
        {
            // arrange
            var StarredPupilConfirmationViewModel = new StarredPupilConfirmationViewModel
            {
                SelectedPupil = _paginatedResultsFake.GetUpn(),
                DownloadType = DownloadType.PupilPremium,
                ConfirmationGiven = false,
                ConfirmationError = true,
                ConfirmationReturnAction = Global.PPDownloadConfirmationReturnAction,
                CancelReturnAction = Global.PPDownloadCancellationReturnAction,
                LearnerNumbers = _paginatedResultsFake.GetUpn()
            };

            _mockDownloadService.GetPupilPremiumCSVFile(
               Arg.Any<string[]>(),
               Arg.Any<string[]>(),
               Arg.Any<bool>(),
               Arg.Any<AzureFunctionHeaderDetails>(),
               Arg.Any<ReturnRoute>(),
               Arg.Any<UserOrganisation>())
               .Returns(new ReturnFile()
               {
                   FileName = "test",
                   FileType = "csv",
                   Bytes = new byte[0]
               });

            // act
            var sut = GetController();
            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());
            var result = await sut.DownloadFileConfirmationReturn(StarredPupilConfirmationViewModel);

            // assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task DownloadPupilPremiumFile_redirects_to_error_when_no_data_available()
        { // arrange
            var upn = _paginatedResultsFake.GetUpn();
            var downloadViewModel = new LearnerDownloadViewModel
            {
                SelectedPupils = upn,
                LearnerNumber = upn,
                SelectedPupilsCount = 1,
                DownloadFileType = DownloadFileType.CSV,
                ShowTABDownloadType = true
            };

            _mockDownloadService.GetPupilPremiumCSVFile(
               Arg.Any<string[]>(),
               Arg.Any<string[]>(),
               Arg.Any<bool>(),
               Arg.Any<AzureFunctionHeaderDetails>(),
               Arg.Any<ReturnRoute>())
               .Returns(new ReturnFile()
               {
                   FileName = null,
                   FileType = null,
                   Bytes = null
               });

            // act
            var sut = GetController();
            var result = await sut.DownloadPupilPremiumFile(downloadViewModel);

            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task ToDownloadSelectedPupilPremiumDataUPN_returns_search_page_with_error_if_no_pupil_selected()
        {
            // arrange
            var upn = string.Empty;
            var searchText = "John Smith";
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());
            _mockSelectionManager.GetSelectedFromSession().Returns(upn);

            // act
            var sut = GetController();
            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            sut.TempData = Substitute.For<ITempDataDictionary>();
            var result = await sut.ToDownloadSelectedPupilPremiumDataUPN(searchViewModel);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerTextSearchViewModel;
            AssertAbstractValues(sut, model);
            Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
            Assert.True(model.NoPupil);
            Assert.True(model.NoPupilSelected);
        }

        [Fact]
        public async Task ToDownloadSelectedPupilPremiumDataUPN_returns_starred_pupil_confirmation_if_starred_pupil_selected()
        {
            // arrange
            var upn = _paginatedResultsFake.GetBase64EncodedUpn();
            var searchText = "John Smith";
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());
            _mockSelectionManager.GetSelectedFromSession().Returns(upn);

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.ToDownloadSelectedPupilPremiumDataUPN(searchViewModel);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerTextSearchViewModel;
            var starredPupilViewModel = model.StarredPupilConfirmationViewModel;
            Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
            Assert.Equal(upn, starredPupilViewModel.SelectedPupil);
        }

        [Theory]
        [InlineData(DownloadFileType.None, new byte[0], SearchErrorMessages.SelectFileType)]
        [InlineData(DownloadFileType.CSV, new byte[0], SearchErrorMessages.SelectOneOrMoreDataTypes)]
        [InlineData(DownloadFileType.CSV, null, DownloadErrorMessages.NoDataForSelectedPupils)]
        public async Task ToDownloadSelectedPupilPremiumDataUPN_returns_correct_validation_error_message(DownloadFileType downloadFileType, byte[] fileBytes, string errorMessage)
        {
            // arrange
            var upn = _paginatedResultsFake.GetUpn();
            var downloadViewModel = new LearnerDownloadViewModel
            {
                SelectedPupils = upn,
                LearnerNumber = upn,
                SelectedPupilsCount = 1,
                DownloadFileType = downloadFileType,
                ShowTABDownloadType = true
            };

            _mockDownloadService.GetPupilPremiumCSVFile(
               Arg.Any<string[]>(),
               Arg.Any<string[]>(),
               Arg.Any<bool>(),
               Arg.Any<AzureFunctionHeaderDetails>(),
               Arg.Any<ReturnRoute>())
               .Returns(new ReturnFile()
               {
                   FileName = "test",
                   FileType = "csv",
                   Bytes = fileBytes
               });

            ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
            TempDataDictionaryFactory tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
            ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

            // act
            var sut = GetController();
            sut.TempData = tempData;

            var result = await sut.DownloadPupilPremiumFile(downloadViewModel);

            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task DownloadCancellationReturn_redirects_to_search()
        {
            // arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

            // act
            var sut = GetController();
            _mockSession.SetString(sut.SearchSessionKey, searchText);
            _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.DownloadCancellationReturn(new StarredPupilConfirmationViewModel());

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.NonUpnSearchView));

            Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerTextSearchViewModel;

            AssertAbstractValues(sut, model);
            AssertContentServicePublicationValues(model);
            Assert.Equal(searchText, model.SearchText);
            Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        }

        [Fact]
        public async Task DownloadCancellationReturn_redirects_to_search_sets_download_links()
        {
            // arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

            // act
            var sut = GetController();
            _mockSession.SetString(sut.SearchSessionKey, searchText);
            _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.DownloadCancellationReturn(new StarredPupilConfirmationViewModel());

            // assert            
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
            Assert.Equal(ApplicationLabel.DownloadSelectedPupilPremiumDataLink, model.DownloadSelectedLink);
            Assert.Equal(ApplicationLabel.AddSelectedToMyPupilListLink, model.AddSelectedToMyPupilListLink);

        }

        #endregion Download

        #region Invalid UPNs

        [Fact]
        public async Task PPNonUpnInvalidUPNs_returns_invalid_upn_page_upns_only()
        {
            // Arrange
            var upn = _paginatedResultsFake.GetInvalidUpn();
            var invalidLearnerNumberSearchViewModel = new InvalidLearnerNumberSearchViewModel()
            {
                LearnerNumber = upn
            };

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Numbers, _paginatedResultsFake.GetInvalidLearners());
            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Id, new PaginatedResponse());

            var result = await sut.PPNonUpnInvalidUPNs(invalidLearnerNumberSearchViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var vm = Assert.IsType<InvalidLearnerNumberSearchViewModel>(viewResult.Model);
            Assert.Equal(Global.InvalidUPNsView, viewResult.ViewName);
            Assert.True(vm.Learners.SequenceEqual(_paginatedResultsFake.GetInvalidLearners().Learners));
        }

        [Fact]
        public async Task PPNonUpnInvalidUPNs_returns_invalid_upn_page_ids_and_upns()
        {
            // Arrange
            var upn = _paginatedResultsFake.GetInvalidUpn();
            var invalidLearnerNumberSearchViewModel = new InvalidLearnerNumberSearchViewModel()
            {
                LearnerNumber = upn
            };

            var expectedLearners = _paginatedResultsFake.GetInvalidLearners().Learners.Concat(_paginatedResultsFake.GetValidLearners().Learners);

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Numbers, _paginatedResultsFake.GetInvalidLearners());
            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Id, _paginatedResultsFake.GetValidLearners());

            var result = await sut.PPNonUpnInvalidUPNs(invalidLearnerNumberSearchViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var vm = Assert.IsType<InvalidLearnerNumberSearchViewModel>(viewResult.Model);
            Assert.Equal(Global.InvalidUPNsView, viewResult.ViewName);
            Assert.True(vm.Learners.SequenceEqual(expectedLearners));
        }

        [Fact]
        public async Task PPNonUpnInvalidUPNs_returns_invalid_upn_page_ids_only()
        {
            // Arrange
            var upn = _paginatedResultsFake.GetInvalidUpn();
            var invalidLearnerNumberSearchViewModel = new InvalidLearnerNumberSearchViewModel()
            {
                LearnerNumber = upn
            };

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Numbers, new PaginatedResponse());
            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Id, _paginatedResultsFake.GetInvalidLearners());

            var result = await sut.PPNonUpnInvalidUPNs(invalidLearnerNumberSearchViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var vm = Assert.IsType<InvalidLearnerNumberSearchViewModel>(viewResult.Model);
            Assert.Equal(Global.InvalidUPNsView, viewResult.ViewName);
            Assert.True(vm.Learners.SequenceEqual(_paginatedResultsFake.GetInvalidLearners().Learners));
        }

        [Fact]
        public async Task PPNonUpnInvalidUPNsConfirmation_redirects_to_my_pupil_list()
        {
            // Arrange
            var upn = _paginatedResultsFake.GetInvalidUpn();
            var invalidLearnerNumberSearchViewModel = new InvalidLearnerNumberSearchViewModel()
            {
                LearnerNumber = upn,
                SelectedInvalidUPNOption = Global.InvalidUPNConfirmation_MyPupilList
            };

            // act
            var sut = GetController();

            var result = await sut.PPNonUpnInvalidUPNsConfirmation(invalidLearnerNumberSearchViewModel);

            // Assert
            var viewResult = Assert.IsAssignableFrom<RedirectToActionResult>(result);
            Assert.Equal(Global.MyPupilListControllerName, viewResult.ControllerName);
            Assert.Equal(Global.MyPupilListAction, viewResult.ActionName);
        }

        [Fact]
        public async Task PPNonUpnInvalidUPNsConfirmation_redirects_to_search()
        {
            // Arrange
            var upn = _paginatedResultsFake.GetInvalidUpn();
            var invalidLearnerNumberSearchViewModel = new InvalidLearnerNumberSearchViewModel()
            {
                LearnerNumber = upn,
                SelectedInvalidUPNOption = Global.InvalidUPNConfirmation_ReturnToSearch
            };

            // act
            var sut = GetController();

            var result = await sut.PPNonUpnInvalidUPNsConfirmation(invalidLearnerNumberSearchViewModel);

            // Assert
            var viewResult = Assert.IsAssignableFrom<RedirectToActionResult>(result);
            Assert.Equal(Global.PPNonUpnAction, viewResult.ActionName);
        }

        [Fact]
        public async Task PPNonUpnInvalidUPNsConfirmation_returns_no_option_selected_validation_message()
        {
            // Arrange
            var upn = _paginatedResultsFake.GetInvalidUpn();
            var invalidLearnerNumberSearchViewModel = new InvalidLearnerNumberSearchViewModel()
            {
                LearnerNumber = upn,
                SelectedInvalidUPNOption = string.Empty
            };

            // act
            var sut = GetController();

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Numbers, _paginatedResultsFake.GetInvalidLearners());
            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Id, new PaginatedResponse());

            MockModelState(invalidLearnerNumberSearchViewModel, sut);

            var result = await sut.PPNonUpnInvalidUPNsConfirmation(invalidLearnerNumberSearchViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.IsType<InvalidLearnerNumberSearchViewModel>(viewResult.Model);
            Assert.Equal(Global.InvalidUPNsView, viewResult.ViewName);
            Assert.True(sut.ViewData.ModelState["NoContinueSelection"].Errors.Count == 1);
        }

        #endregion Invalid UPNs

        #region Sorting

        [Theory]
        [InlineData("Forename", "asc")]
        [InlineData("Surname", "desc")]
        public async Task Sort_is_correctly_handled(string sortField, string sortDirection)
        {
            // arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());
            string surnameFilter = null;
            string middlenameFilter = null;
            string forenameFilter = null;
            string searchByRemove = null;

            // act
            var sut = GetController();
            _mockSession.SetString(sut.SearchSessionKey, searchText);
            _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.NonUpnPupilPremiumDatabase(searchViewModel, surnameFilter, middlenameFilter, forenameFilter, searchByRemove, sortField, sortDirection);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.NonUpnSearchView));

            Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerTextSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.Equal(searchText, model.SearchText);
            Assert.Equal(sortField, model.SortField);
            Assert.Equal(sortDirection, model.SortDirection);
        }

        [Fact]
        public async Task Sort_is_remembered_when_page_number_moves()
        {
            // arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());
            string surnameFilter = null;
            string middlenameFilter = null;
            string forenameFilter = null;
            string searchByRemove = null;

            string sortField = "Forename";
            string sortDirection = "asc";

            // act
            var sut = GetController();
            _mockSession.SetString(sut.SearchSessionKey, searchText);
            _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

            _mockSession.SetString(sut.SortDirectionKey, sortDirection);
            _mockSession.SetString(sut.SortFieldKey, sortField);

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.NonUpnPupilPremiumDatabase(searchViewModel, surnameFilter, middlenameFilter, forenameFilter, searchByRemove, null, null);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.NonUpnSearchView));

            Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerTextSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.Equal(searchText, model.SearchText);
            Assert.Equal(sortField, model.SortField);
            Assert.Equal(sortDirection, model.SortDirection);
        }

        [Fact]
        public async Task Sort_is_remembered_when_returning_to_search()
        {
            // arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

            string sortField = "Forename";
            string sortDirection = "asc";

            // act
            var sut = GetController();
            _mockSession.SetString(sut.SearchSessionKey, searchText);
            _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

            _mockSession.SetString(sut.SortDirectionKey, sortDirection);
            _mockSession.SetString(sut.SortFieldKey, sortField);

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.NonUpnPupilPremiumDatabase(true);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.NonUpnSearchView));

            Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerTextSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.Equal(searchText, model.SearchText);
            Assert.Equal(sortField, model.SortField);
            Assert.Equal(sortDirection, model.SortDirection);
        }

        [Fact]
        public async Task Sort_is_cleared_when_page_is_reset()
        {
            // arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());
            string surnameFilter = null;
            string middlenameFilter = null;
            string forenameFilter = null;
            string searchByRemove = null;
            string sortField = "Forename";
            string sortDirection = "asc";

            // act
            var sut = GetController();
            _mockSession.SetString(sut.SearchSessionKey, searchText);
            _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

            _mockSession.SetString(sut.SortDirectionKey, sortDirection);
            _mockSession.SetString(sut.SortFieldKey, sortField);

            sut.ControllerContext.HttpContext.Request.Query = Substitute.For<IQueryCollection>();
            sut.ControllerContext.HttpContext.Request.Query.ContainsKey("reset").Returns(true);

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.NonUpnPupilPremiumDatabase(searchViewModel, surnameFilter, middlenameFilter, forenameFilter, searchByRemove, null, null);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.NonUpnSearchView));

            Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerTextSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.Equal(searchText, model.SearchText);
            Assert.Null(model.SortField);
            Assert.Null(model.SortDirection);
        }

        [Fact]
        public async Task Sort_is_cleared_when_filters_are_set()
        {
            // Arrange
            SetupContentServicePublicationSchedule();
            var searchText = "John Smith";
            var surnameFilter = "Surname";
            var searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SortDirectionKey, "asc");
            _mockSession.SetString(sut.SortFieldKey, "Forename");

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.SurnameFilter(searchViewModel, surnameFilter);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var model = viewResult.Model as LearnerTextSearchViewModel;

            Assert.Null(model.SortField);
            Assert.Null(model.SortDirection);
        }

        [Fact]
        public async Task Sort_is_cleared_when_filters_are_removed()
        {
            // Arrange
            SetupContentServicePublicationSchedule();
            const string searchText = "John Smith";
            const string surnameFilter = "";
            const string middlenameFilter = null;
            const string forenameFilter = null;
            const string searchByRemove = "Male";

            LearnerTextSearchViewModel searchViewModel =
                SetupLearnerTextSearchViewModel(
                    searchText, _searchFiltersFake.GetSearchFilters(), selectedGenderValues: new string[] { "M" });

            ITempDataDictionary mockTempDataDictionary = Substitute.For<ITempDataDictionary>();
            mockTempDataDictionary.Add("PersistedSelectedGenderFilters", searchByRemove);
            PPLearnerTextSearchController sut = GetController();
            sut.TempData = mockTempDataDictionary;

            // act.
            _mockSession.SetString(sut.SortDirectionKey, "asc");
            _mockSession.SetString(sut.SortFieldKey, "Forename");

            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

            var result = await sut.NonUpnPupilPremiumDatabase(searchViewModel, surnameFilter, middlenameFilter, forenameFilter, searchByRemove, "", "");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var model = viewResult.Model as LearnerTextSearchViewModel;

            Assert.True(String.IsNullOrEmpty(model.SortField));
            Assert.True(String.IsNullOrEmpty(model.SortDirection));
        }

        #endregion Sorting

        #region Private Methods

        private LearnerTextSearchViewModel SetupLearnerTextSearchViewModel(string searchText, SearchFilters searchFilters, string[] selectedGenderValues = null)
        {
            return new LearnerTextSearchViewModel()
            {
                SearchText = searchText,
                SearchFilters = searchFilters,
                SelectedGenderValues = selectedGenderValues
            };
        }

        private void SetupPaginatedSearch(AzureSearchIndexType indexType, AzureSearchQueryType azureSearchQueryType, PaginatedResponse paginatedResponse)
        {
            _mockPaginatedService.GetPage(
           Arg.Any<string>(),
           Arg.Any<Dictionary<string, string[]>>(),
           Arg.Any<int>(),
           Arg.Any<int>(),
           Arg.Is(indexType),
           Arg.Is(azureSearchQueryType),
           Arg.Any<AzureFunctionHeaderDetails>(),
           Arg.Any<string>(),
           Arg.Any<string>())
           .Returns(paginatedResponse);
        }

        private void SetupContentServicePublicationSchedule()
        {
            var expectedCommonResponseBody = new CommonResponseBody()
            {
                Id = "PublicationSchedule",
                Title = "Title",
                Body = "Body"
            };
            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(expectedCommonResponseBody);
        }

        private void AssertAbstractValues(PPLearnerTextSearchController controller, LearnerTextSearchViewModel model)
        {
            Assert.Equal(controller.PageHeading, model.PageHeading);
            Assert.Equal(controller.DownloadLinksPartial, model.DownloadLinksPartial);
            Assert.Equal(controller.InvalidUPNsConfirmationAction, model.InvalidUPNsConfirmationAction);
            Assert.Equal(controller.SearchController, model.LearnerTextSearchController);
            Assert.Equal(controller.SearchAction, model.LearnerTextSearchAction);
            Assert.Equal(controller.SearchLearnerNumberController, model.LearnerNumberController);
            Assert.Equal(controller.SearchLearnerNumberAction, model.LearnerNumberAction);
        }

        private void AssertContentServicePublicationValues(LearnerTextSearchViewModel model)
        {
            Assert.Equal("PublicationSchedule", model.DataReleaseTimeTable.NewsPublication.Id);
            Assert.Equal("Body", model.DataReleaseTimeTable.NewsPublication.Body);
        }

        private PPLearnerTextSearchController GetController(int maxMPLLimit = 4000)
        {
            var user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();

            _mockAppSettings = new AzureAppSettings()
            {
                MaximumUPNsPerSearch = 4000,
                DownloadOptionsCheckLimit = 500,
                NonUpnPPMyPupilListLimit = maxMPLLimit,
                MaximumNonUPNResults = 100
            };

            _mockAppOptions.Value.Returns(_mockAppSettings);

            var httpContextStub = new DefaultHttpContext() { User = user, Session = _mockSession };
            var mockTempData = new TempDataDictionary(httpContextStub, Substitute.For<ITempDataProvider>());

            return new PPLearnerTextSearchController(
                 _mockLogger,
                 _mockPaginatedService,
                 _mockMplService,
                 _mockSelectionManager,
                 _mockContentService,
                 _mockDownloadService,
                 _mockAppOptions)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContextStub
                },
                TempData = mockTempData
            };
        }

        private SearchFilters SetDobFilters(int day, int month, int year)
        {
            return new SearchFilters()
            {
                CurrentFiltersApplied = new List<CurrentFilterDetail>()
                    {
                        new CurrentFilterDetail()
                        {
                            FilterType = FilterType.Dob,
                            FilterName = $"{day}/{month}/{year}"
                        }
                    },
                CurrentFiltersAppliedString = "[{\"FilterName\":\"" + day + "/" + month + "/" + year + "\",\"FilterType\":3}]",
                CustomFilterText = new CustomFilterText()
                {
                    DobDay = day,
                    DobMonth = month,
                    DobYear = year
                }
            };
        }

        /*https://bytelanguage.net/2020/07/31/writing-unit-test-for-model-validation/*/

        private void MockModelState<TModel, TController>(TModel model, TController controller) where TController : ControllerBase
        {
            var validationContext = new ValidationContext(model, null, null);
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            foreach (var validationResult in validationResults)
            {
                controller.ModelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage);
            }
        }

        #endregion Private Methods
    }
}