using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI;
using Game.UI.InGame;
using Game.UI.Widgets;
using System.Collections.Generic;
using static Game.Simulation.TerrainSystem;

namespace RealLife
{
    [FileLocation(nameof(RealLife))]
    [SettingsUIGroupOrder(AgeGroup, EducationGroup)]
    [SettingsUIShowGroupName(AgeGroup, EducationGroup)]
    public class Setting : ModSetting
    {
        public const string AgeSection = "Age";
        public const string AgeGroup = "AgeGroup";
        public const string EducationSection = "Education";
        public const string EducationGroup = "EducationGroup";

        public Setting(IMod mod) : base(mod)
        {
            if (child_age_limit == 0) SetDefaults();
        }

        public override void SetDefaults()
        {
            child_age_limit = 14;
            teen_age_limit = 18;
            adult_age_limit = 65;
            child_school_start_age = 6;
            female_life_expectancy = 83;
            male_life_expectancy = 78;
            years_in_college = 4;
            years_in_university = 3;
        }

        [SettingsUISlider(min = 0, max = 10, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
        [SettingsUISection(AgeSection, AgeGroup)]
        public int child_school_start_age { get; set; }

        [SettingsUISlider(min = 10, max = 25, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
        [SettingsUISection(AgeSection, AgeGroup)]
        public int child_age_limit { get; set; }

        [SettingsUISlider(min = 18, max = 40, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
        [SettingsUISection(AgeSection, AgeGroup)]
        public int teen_age_limit { get; set; }

        [SettingsUISlider(min = 50, max = 90, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
        [SettingsUISection(AgeSection, AgeGroup)]
        public int adult_age_limit { get; set; }

        [SettingsUISlider(min = 60, max = 90, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
        [SettingsUISection(AgeSection, AgeGroup)]
        public int female_life_expectancy { get; set; }

        [SettingsUISlider(min = 60, max = 90, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
        [SettingsUISection(AgeSection, AgeGroup)]
        public int male_life_expectancy { get; set; }

        [SettingsUISlider(min = 1, max = 6, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
        [SettingsUISection(EducationSection, EducationGroup)]
        public int years_in_college { get; set; }

        [SettingsUISlider(min = 1, max = 6, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
        [SettingsUISection(EducationSection, EducationGroup)]
        public int years_in_university { get; set; }
    }  

    public class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Setting;
        public LocaleEN(Setting setting)
        {
            m_Setting = setting;
        }
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "Real Life" },
                { m_Setting.GetOptionTabLocaleID(Setting.AgeSection), "Age" },
                { m_Setting.GetOptionTabLocaleID(Setting.EducationSection), "Education" },

                { m_Setting.GetOptionGroupLocaleID(Setting.AgeGroup), "Age Settings" },
                { m_Setting.GetOptionGroupLocaleID(Setting.EducationGroup), "Education Settings" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.child_school_start_age)), "Child School Start Age" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.child_school_start_age)), $"Age the children start going to Elementary School." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.child_age_limit)), "Child Age Limit" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.child_age_limit)), $"Age that children become teens." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.teen_age_limit)), "Teen Age Limit" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.teen_age_limit)), $"Age that teens become adults." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.adult_age_limit)), "Adult Retirement Age" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.adult_age_limit)), $"Age that adults become elderly and retire." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.female_life_expectancy)), "Female Life Expectancy" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.female_life_expectancy)), $"Average age that female citizens can die." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.male_life_expectancy)), "Male Life Expectancy" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.male_life_expectancy)), $"Average age that male citizens can die." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.years_in_college)), "Number of years in college" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.years_in_college)), $"Number of years that it takes for a cim to graduate college. A year is considered one in game day." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.years_in_university)), "Number of years in university" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.years_in_university)), $"Number of years that it takes for a cim to graduate university. A year is considered one in game day." },
            };
        }

        public void Unload()
        {

        }
    }
}
