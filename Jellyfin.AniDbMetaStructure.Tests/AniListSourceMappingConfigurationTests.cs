using System;
using System.Collections.Generic;
using System.Linq;
using Emby.AniDbMetaStructure.AniList;
using Emby.AniDbMetaStructure.AniList.Data;
using Emby.AniDbMetaStructure.Configuration;
using Emby.AniDbMetaStructure.Tests.TestHelpers;
using FluentAssertions;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using NSubstitute;
using NUnit.Framework;

namespace Emby.AniDbMetaStructure.Tests
{
    [TestFixture]
    [Ignore("Anilist support is not working.")]
    public class AniListSourceMappingConfigurationTests
    {
        [Test]
        public void SeriesMappings_DontMapEmptyFields()
        {
            var source = new AniListSeriesData
            {
                Genres = new List<string>(),
                Staff = new GraphQlEdge<AniListStaffData>(new List<AniListStaffData>()),
                Characters = new GraphQlEdge<AniListCharacterData>(new List<AniListCharacterData>()),
                Studios = new GraphQlEdge<AniListStudioData>(new List<AniListStudioData>())
            };

            var target = new MetadataResult<Series>
            {
                Item = new Series()
            };

            var aniListSourceMappingConfiguration =
                new AniListSourceMappingConfiguration(new AniListNameSelector(new ConsoleLogManager()));

            aniListSourceMappingConfiguration.GetSeriesMappings(1, true, true, TitleType.Localized, "en")
                .Where(m => m.CanApply(source, target))
                .Select(m => m.TargetPropertyName)
                .Should()
                .BeEmpty();
        }

        [Test]
        public void SeriesMappings_HasMappingsForAllFields()
        {
            var expectedMappedFields = new[]
            {
                nameof(Series.PremiereDate),
                nameof(Series.EndDate),
                nameof(Series.Name),
                nameof(Series.Overview),
                nameof(Series.CommunityRating),
                nameof(Series.Studios),
                nameof(Series.Genres),
                nameof(Series.Tags),
                nameof(MetadataResult<Series>.People)
            };

             var nameSelector = Substitute.For<IAniListNameSelector>();

            nameSelector.SelectTitle(Arg.Any<AniListTitleData>(), TitleType.Localized, "en")
                .Returns("SelectedTitle");
            nameSelector.SelectName(Arg.Any<AniListPersonNameData>(), TitleType.Localized, "en")
                .Returns("SelectedName");

            var aniListSourceMappingConfiguration =
                new AniListSourceMappingConfiguration(nameSelector);

            aniListSourceMappingConfiguration.GetSeriesMappings(1, true, true, TitleType.Localized, "en")
                .Select(m => m.TargetPropertyName)
                .Should()
                .BeEquivalentTo(expectedMappedFields);
        }

        [Test]
        public void SeriesMappings_MapsAllFields()
        {
            var source = new AniListSeriesData
            {
                Title = new AniListTitleData("SelectedTitle", string.Empty, string.Empty),
                StartDate = new AniListFuzzyDate(2018, 7, 6),
                EndDate = new AniListFuzzyDate(2018, 8, 5),
                Description = "Description",
                AverageScore = 6.7d,
                Genres = new[] { "Genre", "Tags" },
                Studios = new GraphQlEdge<AniListStudioData>(new List<AniListStudioData>
                {
                    new AniListStudioData(true, new AniListStudioData.StudioName("Studio"))
                }),
                Staff = new GraphQlEdge<AniListStaffData>(new[]
                {
                    new AniListStaffData(
                        new AniListStaffData.InnerStaffData(new AniListPersonNameData("Staff", "Name", string.Empty),
                            new AniListImageUrlData("StaffUrl", string.Empty)), "StaffRole")
                }),
                Characters = new GraphQlEdge<AniListCharacterData>(new[]
                {
                    new AniListCharacterData(
                        new AniListCharacterData.InnerCharacterData(new AniListPersonNameData("Character", "Name", string.Empty),
                            new AniListImageUrlData("CharacterUrl", string.Empty)), new[]
                        {
                            new AniListCharacterData.VoiceActorData("JAPANESE",
                                new AniListImageUrlData("VoiceActorUrl", string.Empty),
                                new AniListPersonNameData("VoiceActor", "Name", string.Empty))
                        })
                })
            };

            var target = new MetadataResult<Series>
            {
                Item = new Series()
            };

            var aniListSourceMappingConfiguration =
                new AniListSourceMappingConfiguration(new AniListNameSelector(new ConsoleLogManager()));

            aniListSourceMappingConfiguration.GetSeriesMappings(1, true, true, TitleType.Localized, "en")
                .Select(m => m.CanApply(source, target))
                .All(v => v)
                .Should()
                .BeTrue();

            aniListSourceMappingConfiguration.GetSeriesMappings(1, true, true, TitleType.Localized, "en")
                .Iter(m => m.Apply(source, target));

            target.Item.Name.Should().Be("SelectedTitle");
            target.Item.PremiereDate.Should().Be(new DateTime(2018, 7, 6, 0, 0, 0));
            target.Item.EndDate.Should().Be(new DateTime(2018, 8, 5, 0, 0, 0));
            target.Item.Overview.Should().Be("Description");
            target.Item.Genres.Should().BeEquivalentTo("Genre");
            target.Item.Studios.Should().BeEquivalentTo("Studio");
            target.Item.Tags.Should().BeEquivalentTo("Tags");
            target.Item.CommunityRating.Should().Be(6.7f);

            target.People.Should()
                .BeEquivalentTo(new List<PersonInfo>
                {
                    new PersonInfo
                    {
                        Name = "Staff Name",
                        Role = "StaffRole",
                        ImageUrl = "StaffUrl"
                    },
                    new PersonInfo
                    {
                        Name = "VoiceActor Name",
                        Role = "Character Name",
                        Type = PersonType.Actor,
                        ImageUrl = "VoiceActorUrl"
                    }
                });
        }
    }
}