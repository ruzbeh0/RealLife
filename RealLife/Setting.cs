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
    [SettingsUIGroupOrder(AgeGroup, EducationGroup, CitizenGroup, HouseholdGroup)]
    [SettingsUIShowGroupName(AgeGroup, GraduationGroup, EducationGroup)]
    public class Setting : ModSetting
    {
        public const string AgeSection = "Age";
        public const string AgeGroup = "AgeGroup";
        public const string EducationSection = "Education";
        public const string CitizenSection = "CitizenSection";
        public const string HouseholdSection = "HouseholdSection";
        public const string EducationGroup = "EducationGroup";
        public const string GraduationGroup = "GraduationGroup";
        public const string CitizenGroup = "CitizenGroup";
        public const string HouseholdGroup = "HouseholdGroup";

        public Setting(IMod mod) : base(mod)
        {
            if (child_age_limit == 0) SetDefaults();
        }

        public override void SetDefaults()
        {
            child_age_limit = 14;
            teen_age_limit = 18;
            male_adult_age_limit = 65;
            female_adult_age_limit = 65;
            child_school_start_age = 6;
            female_life_expectancy = 83;
            male_life_expectancy = 78;
            years_in_college = 4;
            years_in_university = 3;
            elementary_grad_prob = 100;
            high_grad_prob = 95;
            college_grad_prob = 90;
            university_grad_prob = 85;
            enter_high_school_prob = 88;
            adult_enter_high_school_prob = 10;
            worker_continue_education = 70;
            student_birth_rate_adjuster = 0;
            base_birth_rate_adjuster = 0;
            adult_female_birth_rate_bonus_adjuster = 0;
            divorce_rate_adjuster = 0;
            look_for_partner_rate_adjuster = 0;
            college_edu_in_univ = 0;
            corpse_vanish = 0;
            average_household_size = 2.77f;
            disable_household_deletion = false;
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
        public int male_adult_age_limit { get; set; }

        [SettingsUISlider(min = 50, max = 90, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
        [SettingsUISection(AgeSection, AgeGroup)]
        public int female_adult_age_limit { get; set; }

        [SettingsUISlider(min = 60, max = 90, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
        [SettingsUISection(AgeSection, AgeGroup)]
        public int female_life_expectancy { get; set; }

        [SettingsUISlider(min = 60, max = 90, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
        [SettingsUISection(AgeSection, AgeGroup)]
        public int male_life_expectancy { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, scalarMultiplier = 1, unit = Unit.kPercentage)]
        [SettingsUISection(AgeSection, AgeGroup)]
        public int corpse_vanish { get; set; }

        [SettingsUISlider(min = 1, max = 6, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
        [SettingsUISection(EducationSection, GraduationGroup)]
        public int years_in_college { get; set; }

        [SettingsUISlider(min = 1, max = 6, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
        [SettingsUISection(EducationSection, GraduationGroup)]
        public int years_in_university { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, scalarMultiplier = 1, unit = Unit.kPercentage)]
        [SettingsUISection(EducationSection, GraduationGroup)]
        public int elementary_grad_prob { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, scalarMultiplier = 1, unit = Unit.kPercentage)]
        [SettingsUISection(EducationSection, EducationGroup)]
        public int college_edu_in_univ { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, scalarMultiplier = 1, unit = Unit.kPercentage)]
        [SettingsUISection(EducationSection, GraduationGroup)]
        public int high_grad_prob { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, scalarMultiplier = 1, unit = Unit.kPercentage)]
        [SettingsUISection(EducationSection, GraduationGroup)]
        public int college_grad_prob { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, scalarMultiplier = 1, unit = Unit.kPercentage)]
        [SettingsUISection(EducationSection, GraduationGroup)]
        public int university_grad_prob { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, scalarMultiplier = 1, unit = Unit.kPercentage)]
        [SettingsUISection(EducationSection, EducationGroup)]
        public int enter_high_school_prob { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, scalarMultiplier = 1, unit = Unit.kPercentage)]
        [SettingsUISection(EducationSection, EducationGroup)]
        public int adult_enter_high_school_prob { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, scalarMultiplier = 1, unit = Unit.kPercentage)]
        [SettingsUISection(EducationSection, EducationGroup)]
        public int worker_continue_education { get; set; }

        [SettingsUISlider(min = -100, max = 300, step = 1, scalarMultiplier = 1, unit = Unit.kPercentage)]
        [SettingsUISection(CitizenSection, CitizenGroup)]
        public int student_birth_rate_adjuster { get; set; }

        [SettingsUISlider(min = -100, max = 300, step = 1, scalarMultiplier = 1, unit = Unit.kPercentage)]
        [SettingsUISection(CitizenSection, CitizenGroup)]
        public int base_birth_rate_adjuster { get; set; }

        [SettingsUISlider(min = -100, max = 300, step = 1, scalarMultiplier = 1, unit = Unit.kPercentage)]
        [SettingsUISection(CitizenSection, CitizenGroup)]
        public int divorce_rate_adjuster { get; set; }

        [SettingsUISlider(min = -100, max = 300, step = 1, scalarMultiplier = 1, unit = Unit.kPercentage)]
        [SettingsUISection(CitizenSection, CitizenGroup)]
        public int adult_female_birth_rate_bonus_adjuster { get; set; }

        [SettingsUISlider(min = -100, max = 300, step = 1, scalarMultiplier = 1, unit = Unit.kPercentage)]
        [SettingsUISection(CitizenSection, CitizenGroup)]
        public int look_for_partner_rate_adjuster { get; set; }

        [SettingsUISlider(min = 1f, max = 10f, step = 0.01f, scalarMultiplier = 1, unit = Unit.kFloatTwoFractions)]
        [SettingsUISection(HouseholdSection, HouseholdGroup)]
        public float average_household_size { get; set; }

        [SettingsUISection(HouseholdSection, HouseholdGroup)]
        public bool disable_household_deletion { get; set; }

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
                { m_Setting.GetOptionTabLocaleID(Setting.CitizenSection), "Birth & Relationships" },
                { m_Setting.GetOptionTabLocaleID(Setting.HouseholdSection), "Household" },

                { m_Setting.GetOptionGroupLocaleID(Setting.AgeGroup), "Age Settings" },
                { m_Setting.GetOptionGroupLocaleID(Setting.EducationGroup), "Education" },
                { m_Setting.GetOptionGroupLocaleID(Setting.GraduationGroup), "Graduation" },
                { m_Setting.GetOptionGroupLocaleID(Setting.CitizenGroup), "Birth & Relationships" },
                { m_Setting.GetOptionGroupLocaleID(Setting.HouseholdGroup), "Household" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.child_school_start_age)), "Child School Start Age" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.child_school_start_age)), $"Age the children start going to Elementary School. Vanilla value is Zero." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.child_age_limit)), "Child Age Limit" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.child_age_limit)), $"Age that children become teens. Vanilla value is 21." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.teen_age_limit)), "Teen Age Limit" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.teen_age_limit)), $"Age that teens become adults. Vanilla value is 36." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.male_adult_age_limit)), "Men Retirement Age" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.male_adult_age_limit)), $"Age that men become elderly and retire. Vanilla value is 75." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.female_adult_age_limit)), "Women Retirement Age" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.female_adult_age_limit)), $"Age that women become elderly and retire. Vanilla value is 75." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.female_life_expectancy)), "Female Life Expectancy" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.female_life_expectancy)), $"Average age that female citizens can die." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.male_life_expectancy)), "Male Life Expectancy" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.male_life_expectancy)), $"Average age that male citizens can die." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.corpse_vanish)), "Probability of Vanishing Corpses" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.corpse_vanish)), $"Probability that a corpse will vanish and will not require death care services." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.college_edu_in_univ)), "University Capacity for College" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.college_edu_in_univ)), $"Percentage of University capacity that will be for College education. If higher than zero, some college students will go to for College Degree in an University." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.years_in_college)), "Number of years in college" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.years_in_college)), $"Number of years that it takes for a cim to graduate college. A year is considered one in game day." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.years_in_university)), "Number of years in university" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.years_in_university)), $"Number of years that it takes for a cim to graduate university. A year is considered one in game day." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.elementary_grad_prob)), "Elementary School Graduation Probability" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.elementary_grad_prob)), $"Probability of graduating elementary school. Vanilla is 100%" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.high_grad_prob)), "High School Graduation Probability" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.high_grad_prob)), $"Probability of graduating high school. Vanilla is 60%" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.college_grad_prob)), "College Graduation Probability" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.college_grad_prob)), $"Probability of graduating college. Vanilla is 90%" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.university_grad_prob)), "University Graduation Probability" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.university_grad_prob)), $"Probability of graduating university. Vanilla is 70%" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.enter_high_school_prob)), "Probability of Entering High School" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.enter_high_school_prob)), $"Percentage of citizens that enter High School." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.adult_enter_high_school_prob)), "Probability of Adults Entering High School" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.adult_enter_high_school_prob)), $"Percentage of adults that enter High School." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.worker_continue_education)), "Probability of Workers continuing Education" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.worker_continue_education)), $"Percentage of workers that will continue their education." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.student_birth_rate_adjuster)), "Student Birth Rate Factor" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.student_birth_rate_adjuster)), $"Increase or decrease the student birth rate" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.base_birth_rate_adjuster)), "Base Birth Rate Factor" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.base_birth_rate_adjuster)), $"Increase or decrease the base birth rate" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.adult_female_birth_rate_bonus_adjuster)), "Adult Female Birth Rate Factor" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.adult_female_birth_rate_bonus_adjuster)), $"Increase or decrease the adult female bonus birth rate" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.divorce_rate_adjuster)), "Divorce Rate Factor" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.divorce_rate_adjuster)), $"Increase or decrease the divorce rate" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.look_for_partner_rate_adjuster)), "Look for Partner Rate Factor" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.look_for_partner_rate_adjuster)), $"Increase or decrease the look for partner rate" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.average_household_size)), "Average Household Size" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.average_household_size)), $"This mod will try to keep the average household size in this value by both controlling birth rates and deleting large households (if enabled)." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.disable_household_deletion)), "Disable deletion of large households" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.disable_household_deletion)), $"Disable deletion of large households that are above the average household size. Deletion will only happen if a group of households is above the average. If a household is bigger than the average but it is in a group that is in the average, it will not be deleted." },
            };
        }

        public void Unload()
        {

        }
    }
}
