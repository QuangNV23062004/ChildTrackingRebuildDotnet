using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestAPI.Enums;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Models.SubModels;
using RestAPI.Repositories.interfaces;
using RestAPI.Services.interfaces;
using Sprache;

namespace RestAPI.Services.services
{
    public class GrowthDataService(
        IGrowthDataRepository _growthDataRepository,
        IUserRepository _userRepository,
        IChildRepository _childRepository,
        IGrowthMetricForAgeRepository _growthMetricForAgeRepository,
        IGrowthVelocityRepository _growthVelocityRepository,
        IWflhRepository _wflhRepository
    ) : IGrowthDataService
    {
        // 1. Basic helper with no dependencies
        private double? GetPercentile(double measurement, List<PercentileValue> percentiles)
        {
            if (measurement <= percentiles[0].Value)
                return percentiles[0].Percentile;

            if (measurement >= percentiles[percentiles.Count - 1].Value)
                return percentiles[percentiles.Count - 1].Percentile;

            for (int i = 0; i < percentiles.Count - 1; i++)
            {
                var lower = percentiles[i];
                var upper = percentiles[i + 1];

                if (measurement >= lower.Value && measurement <= upper.Value)
                {
                    double fraction = (measurement - lower.Value) / (upper.Value - lower.Value);
                    double result =
                        lower.Percentile + fraction * (upper.Percentile - lower.Percentile);
                    return Math.Round(result * 100) / 100;
                }
            }

            return null;
        }

        private string GetIntervalDescription(Interval first, Interval last)
        {
            if (first.InWeeks == 0 && last.InWeeks == 4)
                return "0 - 4 weeks";
            if (first.InWeeks == 4 && last.InMonths == 2)
                return "4 weeks - 2 months";
            return $"{first.InMonths} - {last.InMonths} months";
        }

        private double? CalculateMetricVelocity(
            double? startValue,
            double? endValue,
            double timeDiffMonths
        )
        {
            if (!startValue.HasValue || !endValue.HasValue || timeDiffMonths <= 0)
                return null;

            return (endValue.Value - startValue.Value) / timeDiffMonths;
        }

        private GrowthDataModel FindClosestGrowthData(
            List<GrowthDataModel> growthData,
            double targetDays,
            DateTime birthDate
        )
        {
            var targetDate = birthDate.AddDays(targetDays);
            GrowthDataModel closestData = null;
            double minDiff = double.MaxValue;

            foreach (var data in growthData)
            {
                double dataDays = (data.InputDate - birthDate).TotalDays;
                double diff = Math.Abs(dataDays - targetDays);

                if (diff < minDiff)
                {
                    closestData = data;
                    minDiff = diff;
                }
            }

            return closestData;
        }

        private string formatPercentile(double percentile)
        {
            return percentile % 1 == 0 ? $"{percentile}" : percentile.ToString("F2");
        }

        private LevelEnum DetermineLevel(double percentile)
        {
            // If percentile is -1, it means no data available
            if (percentile == -1)
                return LevelEnum.NA;

            if (percentile < 5)
                return LevelEnum.Low;
            else if (percentile >= 5 && percentile < 15)
                return LevelEnum.BelowAverage;
            else if (percentile >= 15 && percentile < 95)
                return LevelEnum.Average;
            else if (percentile >= 95)
                return LevelEnum.AboveAverage;
            else
                return LevelEnum.NA;
        }

        private string FormatPeriod(double firstMonth, double lastMonth)
        {
            // Round to 1 decimal place for better readability
            var firstFormatted = Math.Round(firstMonth, 1);
            var lastFormatted = Math.Round(lastMonth, 1);

            // If both are whole numbers, display as integers
            if (firstFormatted % 1 == 0 && lastFormatted % 1 == 0)
            {
                return $"{(int)firstFormatted} - {(int)lastFormatted} months";
            }

            return $"{firstFormatted} - {lastFormatted} months";
        }

        private async Task<GrowthResult> generateGrowthResult(
            GrowthDataModel growthData,
            ChildModel child
        )
        {
            Console.WriteLine(
                $"[generateGrowthResult] Starting calculation for child: {child.Name}"
            );
            Console.WriteLine(
                $"[generateGrowthResult] Input data - Height: {growthData.Height}, Weight: {growthData.Weight}, HeadCircumference: {growthData.HeadCircumference}"
            );
            Console.WriteLine(
                $"[generateGrowthResult] Child info - Gender: {child.Gender}, BirthDate: {child.BirthDate}"
            );

            double conversionRate = 30.4375;

            var today = growthData.InputDate.ToUniversalTime().Ticks;
            var birth = child.BirthDate.ToUniversalTime().Ticks;

            var diffInDays = TimeSpan.FromTicks(today - birth).TotalDays;
            double diffInWeeks = diffInDays / 7;
            double diffInMonths = diffInDays / conversionRate;

            var ageInDays = Math.Round(diffInDays);
            var ageInWeeks = Math.Round(diffInWeeks);
            var ageInMonths = Math.Round(diffInMonths);

            Console.WriteLine(
                $"[generateGrowthResult] Age calculated - Days: {ageInDays}, Weeks: {ageInWeeks}, Months: {ageInMonths}"
            );

            List<GrowthMetricForAgeModel> growthMetricsForAgeData;
            if (ageInDays <= 1856)
            {
                Console.WriteLine($"[generateGrowthResult] Using daily metrics (age <= 1856 days)");
                growthMetricsForAgeData =
                    await _growthMetricForAgeRepository.GetGrowthMetricsForAgeData(
                        (int)child.Gender,
                        (int)Math.Round(ageInDays),
                        "day"
                    );
            }
            else
            {
                Console.WriteLine(
                    $"[generateGrowthResult] Using monthly metrics (age > 1856 days)"
                );
                growthMetricsForAgeData =
                    await _growthMetricForAgeRepository.GetGrowthMetricsForAgeData(
                        (int)child.Gender,
                        (int)Math.Round(ageInMonths),
                        "month"
                    );
            }

            Console.WriteLine(
                $"[generateGrowthResult] Retrieved {growthMetricsForAgeData.Count} growth metrics for age data"
            );

            var height = growthData.Height;
            var weight = growthData.Weight;
            var headCircumference = growthData.HeadCircumference;
            var armCircumference = growthData.ArmCircumference;

            decimal bmi = (decimal)(weight / (height * height) * 10000);
            Console.WriteLine($"[generateGrowthResult] Calculated BMI: {bmi}");

            var growthResult = new GrowthResult
            {
                Height = new GrowthMetric
                {
                    Percentile = -1,
                    Description = "N/A",
                    Level = LevelEnum.NA,
                },
                Weight = new GrowthMetric
                {
                    Percentile = -1,
                    Description = "N/A",
                    Level = LevelEnum.NA,
                },
                Bmi = new BmiMetric
                {
                    Percentile = -1,
                    Description = "N/A",
                    Level = BmiLevelEnum.NA,
                },
                HeadCircumference = new GrowthMetric
                {
                    Percentile = -1,
                    Description = "N/A",
                    Level = LevelEnum.NA,
                },
                ArmCircumference = new GrowthMetric
                {
                    Percentile = -1,
                    Description = "N/A",
                    Level = LevelEnum.NA,
                },
                WeightForLength = new GrowthMetric
                {
                    Percentile = -1,
                    Description = "N/A",
                    Level = LevelEnum.NA,
                },
            };
            var irregular = false;

            Console.WriteLine(
                $"[generateGrowthResult] Processing {growthMetricsForAgeData.Count} growth metrics..."
            );
            growthMetricsForAgeData.ForEach(data =>
            {
                Console.WriteLine($"[generateGrowthResult] Processing metric type: {data.Type}");
                switch (data.Type)
                {
                    case GrowthMetricsForAgeEnum.BFA:
                    {
                        var percentile = GetPercentile((double)bmi, data.Percentiles.Values);
                        Console.WriteLine(
                            $"[generateGrowthResult] BMI percentile calculated: {percentile}"
                        );
                        if (percentile.HasValue)
                        {
                            growthResult!.Bmi!.Percentile = percentile.Value;
                            var genderString =
                                ((GenderEnum)child.Gender == GenderEnum.Boy) ? "boys" : "girls";
                            growthResult!.Bmi!.Description =
                                $"Your child is in the {percentile.Value} percentile for BMI. That means {percentile.Value} percent of {genderString} at that age have a lower BMI, while {formatPercentile(100 - percentile.Value)} percent have a higher BMI.";

                            if (percentile.Value < 5)
                            {
                                growthResult!.Bmi!.Level = BmiLevelEnum.Underweight;
                                irregular = true;
                                Console.WriteLine(
                                    $"[generateGrowthResult] BMI level set to Underweight (irregular)"
                                );
                            }
                            else if (percentile.Value >= 5 && percentile.Value < 15)
                            {
                                growthResult!.Bmi!.Level = BmiLevelEnum.HealthyWeight;
                                Console.WriteLine(
                                    $"[generateGrowthResult] BMI level set to HealthyWeight"
                                );
                            }
                            else if (percentile.Value >= 15 && percentile.Value < 95)
                            {
                                growthResult!.Bmi!.Level = BmiLevelEnum.Overweight;
                                Console.WriteLine(
                                    $"[generateGrowthResult] BMI level set to Overweight"
                                );
                            }
                            else if (percentile.Value >= 95)
                            {
                                growthResult!.Bmi!.Level = BmiLevelEnum.Obese;
                                irregular = true;
                                Console.WriteLine(
                                    $"[generateGrowthResult] BMI level set to Obese (irregular)"
                                );
                            }
                        }
                        break;
                    }
                    case GrowthMetricsForAgeEnum.LHFA:
                    {
                        var percentile = GetPercentile((double)height!, data.Percentiles.Values);
                        Console.WriteLine(
                            $"[generateGrowthResult] Height percentile calculated: {percentile}"
                        );
                        if (percentile.HasValue)
                        {
                            growthResult!.Height!.Percentile = percentile.Value;
                            var genderString =
                                ((GenderEnum)child.Gender == GenderEnum.Boy) ? "boys" : "girls";
                            growthResult!.Height!.Description =
                                $"Your child is in the {percentile.Value} percentile for height. That means {percentile.Value} percent of {genderString} at that age are shorter, while {formatPercentile(100 - percentile.Value)} percent are taller.";

                            if (percentile.Value < 5)
                            {
                                growthResult!.Height!.Level = LevelEnum.Low;
                                Console.WriteLine(
                                    $"[generateGrowthResult] Height level set to Low"
                                );
                            }
                            else if (percentile.Value >= 5 && percentile.Value < 15)
                            {
                                growthResult!.Height!.Level = LevelEnum.BelowAverage;
                                Console.WriteLine(
                                    $"[generateGrowthResult] Height level set to BelowAverage"
                                );
                            }
                            else if (percentile.Value >= 15 && percentile.Value < 95)
                            {
                                growthResult!.Height!.Level = LevelEnum.Average;
                                Console.WriteLine(
                                    $"[generateGrowthResult] Height level set to Average"
                                );
                            }
                            else if (percentile.Value >= 95)
                            {
                                growthResult!.Height!.Level = LevelEnum.AboveAverage;
                                Console.WriteLine(
                                    $"[generateGrowthResult] Height level set to AboveAverage"
                                );
                            }
                        }
                        break;
                    }
                    case GrowthMetricsForAgeEnum.WFA:
                    {
                        var percentile = GetPercentile((double)weight!, data.Percentiles.Values);
                        Console.WriteLine(
                            $"[generateGrowthResult] Weight percentile calculated: {percentile}"
                        );
                        if (percentile.HasValue)
                        {
                            growthResult!.Weight!.Percentile = percentile.Value;
                            var genderString =
                                ((GenderEnum)child.Gender == GenderEnum.Boy) ? "boys" : "girls";
                            growthResult!.Weight!.Description =
                                $"Your child is in the {percentile.Value} percentile for weight. That means {percentile.Value} percent of {genderString} at that age weigh less, while {formatPercentile(100 - percentile.Value)} percent weigh more.";

                            if (percentile.Value < 5)
                            {
                                growthResult!.Weight!.Level = LevelEnum.Low;
                                Console.WriteLine(
                                    $"[generateGrowthResult] Weight level set to Low"
                                );
                            }
                            else if (percentile.Value >= 5 && percentile.Value < 15)
                            {
                                growthResult!.Weight!.Level = LevelEnum.BelowAverage;
                                Console.WriteLine(
                                    $"[generateGrowthResult] Weight level set to BelowAverage"
                                );
                            }
                            else if (percentile.Value >= 15 && percentile.Value < 95)
                            {
                                growthResult!.Weight!.Level = LevelEnum.Average;
                                Console.WriteLine(
                                    $"[generateGrowthResult] Weight level set to Average"
                                );
                            }
                            else if (percentile.Value >= 95)
                            {
                                growthResult!.Weight!.Level = LevelEnum.AboveAverage;
                                irregular = true;
                                Console.WriteLine(
                                    $"[generateGrowthResult] Weight level set to AboveAverage (irregular)"
                                );
                            }
                        }
                        break;
                    }
                    case GrowthMetricsForAgeEnum.HCFA:
                    {
                        {
                            if (headCircumference == null || headCircumference is not double)
                            {
                                Console.WriteLine(
                                    $"[generateGrowthResult] Head circumference is null or not double, setting to N/A"
                                );
                                growthResult!.HeadCircumference!.Percentile = -1;
                                growthResult!.HeadCircumference!.Description = "N/A";
                                growthResult!.HeadCircumference!.Level = LevelEnum.NA;
                                break;
                            }
                        }

                        var percentile = GetPercentile(
                            (double)headCircumference!,
                            data.Percentiles.Values
                        );
                        Console.WriteLine(
                            $"[generateGrowthResult] Head circumference percentile calculated: {percentile}"
                        );
                        if (percentile.HasValue)
                        {
                            growthResult!.HeadCircumference!.Percentile = percentile.Value;
                            var genderString =
                                ((GenderEnum)child.Gender == GenderEnum.Boy) ? "boys" : "girls";
                            growthResult!.HeadCircumference!.Description =
                                $"Your child is in the {percentile.Value} percentile for head circumference. That means {percentile.Value} percent of {genderString} at that age have a smaller head, while {formatPercentile(100 - percentile.Value)} percent have a larger head.";

                            if (percentile.Value < 5)
                            {
                                growthResult!.HeadCircumference!.Level = LevelEnum.Low;
                                Console.WriteLine(
                                    $"[generateGrowthResult] Head circumference level set to Low"
                                );
                            }
                            else if (percentile.Value >= 5 && percentile.Value < 15)
                            {
                                growthResult!.HeadCircumference!.Level = LevelEnum.BelowAverage;
                                Console.WriteLine(
                                    $"[generateGrowthResult] Head circumference level set to BelowAverage"
                                );
                            }
                            else if (percentile.Value >= 15 && percentile.Value < 95)
                            {
                                growthResult!.HeadCircumference!.Level = LevelEnum.Average;
                                Console.WriteLine(
                                    $"[generateGrowthResult] Head circumference level set to Average"
                                );
                            }
                            else if (percentile.Value >= 95)
                            {
                                growthResult!.HeadCircumference!.Level = LevelEnum.AboveAverage;
                                Console.WriteLine(
                                    $"[generateGrowthResult] Head circumference level set to AboveAverage"
                                );
                            }
                        }
                        break;
                    }
                    case GrowthMetricsForAgeEnum.ACFA:
                    {
                        if (armCircumference == null || (armCircumference is not double))
                        {
                            Console.WriteLine(
                                $"[generateGrowthResult] Arm circumference is null or not double, setting to N/A"
                            );
                            growthResult!.ArmCircumference!.Percentile = -1;
                            growthResult!.ArmCircumference!.Description = "N/A";
                            growthResult!.ArmCircumference!.Level = LevelEnum.NA;
                            break;
                        }

                        var percentile = GetPercentile(
                            (double)armCircumference!,
                            data.Percentiles.Values
                        );
                        Console.WriteLine(
                            $"[generateGrowthResult] Arm circumference percentile calculated: {percentile}"
                        );
                        if (percentile.HasValue)
                        {
                            growthResult!.ArmCircumference!.Percentile = percentile.Value;
                            var genderString =
                                ((GenderEnum)child.Gender == GenderEnum.Boy) ? "boys" : "girls";
                            growthResult!.ArmCircumference!.Description =
                                $"Your child is in the {percentile.Value} percentile for arm circumference. That means {percentile.Value} percent of {genderString} at that age have a smaller arm, while {formatPercentile(100 - percentile.Value)} percent have a larger arm.";

                            if (percentile.Value < 5)
                            {
                                growthResult!.ArmCircumference!.Level = LevelEnum.Low;
                                Console.WriteLine(
                                    $"[generateGrowthResult] Arm circumference level set to Low"
                                );
                            }
                            else if (percentile.Value >= 5 && percentile.Value < 15)
                            {
                                growthResult!.ArmCircumference!.Level = LevelEnum.BelowAverage;
                                Console.WriteLine(
                                    $"[generateGrowthResult] Arm circumference level set to BelowAverage"
                                );
                            }
                            else if (percentile.Value >= 15 && percentile.Value < 95)
                            {
                                growthResult!.ArmCircumference!.Level = LevelEnum.Average;
                                Console.WriteLine(
                                    $"[generateGrowthResult] Arm circumference level set to Average"
                                );
                            }
                            else if (percentile.Value >= 95)
                            {
                                growthResult!.ArmCircumference!.Level = LevelEnum.AboveAverage;
                                Console.WriteLine(
                                    $"[generateGrowthResult] Arm circumference level set to AboveAverage"
                                );
                            }
                        }
                        break;
                    }
                }
            });

            Console.WriteLine(
                $"[generateGrowthResult] Fetching WFLH data for height: {height}, gender: {child.Gender}"
            );
            var wflhData = await _wflhRepository.GetWflhData((double)height!, (int)child.Gender);
            Console.WriteLine(
                $"[generateGrowthResult] Retrieved {wflhData.Count} WFLH data records"
            );

            wflhData.ForEach(data =>
            {
                var percentile = GetPercentile((double)weight!, data.Percentiles.Values);
                Console.WriteLine(
                    $"[generateGrowthResult] Weight for length percentile calculated: {percentile}"
                );
                growthResult!.WeightForLength!.Percentile = percentile ?? -1;
                var genderString = ((GenderEnum)child.Gender == GenderEnum.Boy) ? "boys" : "girls";
                growthResult!.WeightForLength!.Description =
                    $"Your child is in the {percentile} percentile for weight for length. That means {percentile} percent of {genderString} at that age have a lower weight for length, while {formatPercentile(100 - percentile ?? 0)} percent have a higher weight for length.";

                if (percentile < 5)
                {
                    growthResult!.WeightForLength!.Level = LevelEnum.Low;
                    Console.WriteLine($"[generateGrowthResult] Weight for length level set to Low");
                }
                else if (percentile >= 5 && percentile < 15)
                {
                    growthResult!.WeightForLength!.Level = LevelEnum.BelowAverage;
                    Console.WriteLine(
                        $"[generateGrowthResult] Weight for length level set to BelowAverage"
                    );
                }
                else if (percentile >= 15 && percentile < 95)
                {
                    growthResult!.WeightForLength!.Level = LevelEnum.Average;
                    Console.WriteLine(
                        $"[generateGrowthResult] Weight for length level set to Average"
                    );
                }
                else if (percentile >= 95)
                {
                    growthResult!.WeightForLength!.Level = LevelEnum.AboveAverage;
                    irregular = true;
                    Console.WriteLine(
                        $"[generateGrowthResult] Weight for length level set to AboveAverage (irregular)"
                    );
                }
            });

            Console.WriteLine($"[generateGrowthResult] Final irregular status: {irregular}");
            Console.WriteLine($"[generateGrowthResult] Growth result calculation completed");
            return growthResult;
        }

        private async Task<GrowthResult> generateGrowthResult2(
            GrowthDataModel growthData,
            DateTime birthDate,
            int gender
        )
        {
            double conversionRate = 30.4375;

            var today = growthData.InputDate.ToUniversalTime().Ticks;
            var birth = birthDate.ToUniversalTime().Ticks;

            var diffInDays = TimeSpan.FromTicks(today - birth).TotalDays;
            var diffInWeeks = diffInDays / 7;
            var diffInMonths = diffInDays / conversionRate;

            var ageInDays = Math.Round(diffInDays);
            var ageInWeeks = Math.Round(diffInWeeks);
            var ageInMonths = Math.Round(diffInMonths);

            List<GrowthMetricForAgeModel> growthMetricsForAgeData;
            if (ageInDays <= 1856)
            {
                growthMetricsForAgeData =
                    await _growthMetricForAgeRepository.GetGrowthMetricsForAgeData(
                        gender,
                        (int)Math.Round(ageInDays),
                        "day"
                    );
            }
            else
            {
                growthMetricsForAgeData =
                    await _growthMetricForAgeRepository.GetGrowthMetricsForAgeData(
                        gender,
                        (int)Math.Round(ageInMonths),
                        "month"
                    );
            }

            var height = growthData.Height;
            var weight = growthData.Weight;
            var headCircumference = growthData.HeadCircumference;
            var armCircumference = growthData.ArmCircumference;

            decimal bmi = (decimal)(weight / (height * height) * 10000);
            var growthResult = new GrowthResult
            {
                Height = new GrowthMetric
                {
                    Percentile = -1,
                    Description = "N/A",
                    Level = LevelEnum.NA,
                },
                Weight = new GrowthMetric
                {
                    Percentile = -1,
                    Description = "N/A",
                    Level = LevelEnum.NA,
                },
                Bmi = new BmiMetric
                {
                    Percentile = -1,
                    Description = "N/A",
                    Level = BmiLevelEnum.NA,
                },
                HeadCircumference = new GrowthMetric
                {
                    Percentile = -1,
                    Description = "N/A",
                    Level = LevelEnum.NA,
                },
                ArmCircumference = new GrowthMetric
                {
                    Percentile = -1,
                    Description = "N/A",
                    Level = LevelEnum.NA,
                },
                WeightForLength = new GrowthMetric
                {
                    Percentile = -1,
                    Description = "N/A",
                    Level = LevelEnum.NA,
                },
            };
            var irregular = false;

            growthMetricsForAgeData.ForEach(data =>
            {
                switch (data.Type)
                {
                    case GrowthMetricsForAgeEnum.BFA:
                    {
                        var percentile = GetPercentile((double)bmi, data.Percentiles.Values);
                        growthResult!.Bmi!.Percentile = percentile ?? -1;
                        var genderString =
                            ((GenderEnum)gender == GenderEnum.Boy) ? "boys" : "girls";
                        growthResult!.Bmi!.Description =
                            $"Your child is in the {percentile} percentile for BMI. That means {percentile} percent of {genderString} at that age have a lower BMI, while {formatPercentile(100 - percentile ?? 0)} percent have a higher BMI.";

                        if (percentile < 5)
                        {
                            growthResult!.Bmi!.Level = BmiLevelEnum.Underweight;
                            irregular = true;
                        }
                        else if (percentile >= 5 && percentile < 15)
                        {
                            growthResult!.Bmi!.Level = BmiLevelEnum.HealthyWeight;
                        }
                        else if (percentile >= 15 && percentile < 95)
                        {
                            growthResult!.Bmi!.Level = BmiLevelEnum.Overweight;
                        }
                        else if (percentile >= 95)
                        {
                            growthResult!.Bmi!.Level = BmiLevelEnum.Obese;
                            irregular = true;
                        }
                        break;
                    }
                    case GrowthMetricsForAgeEnum.LHFA:
                    {
                        var percentile = GetPercentile((double)height!, data.Percentiles.Values);
                        Console.WriteLine(
                            $"[generateGrowthResult] Height percentile calculated: {percentile}"
                        );
                        if (percentile.HasValue)
                        {
                            growthResult!.Height!.Percentile = percentile.Value;
                            var genderString =
                                ((GenderEnum)gender == GenderEnum.Boy) ? "boys" : "girls";
                            growthResult!.Height!.Description =
                                $"Your child is in the {percentile.Value} percentile for height. That means {percentile.Value} percent of {genderString} at that age are shorter, while {formatPercentile(100 - percentile.Value)} percent are taller.";

                            if (percentile.Value < 5)
                            {
                                growthResult!.Height!.Level = LevelEnum.Low;
                                Console.WriteLine(
                                    $"[generateGrowthResult] Height level set to Low"
                                );
                            }
                            else if (percentile.Value >= 5 && percentile.Value < 15)
                            {
                                growthResult!.Height!.Level = LevelEnum.BelowAverage;
                                Console.WriteLine(
                                    $"[generateGrowthResult] Height level set to BelowAverage"
                                );
                            }
                            else if (percentile.Value >= 15 && percentile.Value < 95)
                            {
                                growthResult!.Height!.Level = LevelEnum.Average;
                                Console.WriteLine(
                                    $"[generateGrowthResult] Height level set to Average"
                                );
                            }
                            else if (percentile.Value >= 95)
                            {
                                growthResult!.Height!.Level = LevelEnum.AboveAverage;
                                Console.WriteLine(
                                    $"[generateGrowthResult] Height level set to AboveAverage"
                                );
                            }
                        }
                        break;
                    }
                    case GrowthMetricsForAgeEnum.WFA:
                    {
                        var percentile = GetPercentile((double)weight!, data.Percentiles.Values);
                        Console.WriteLine(
                            $"[generateGrowthResult] Weight percentile calculated: {percentile}"
                        );
                        if (percentile.HasValue)
                        {
                            growthResult!.Weight!.Percentile = percentile.Value;
                            var genderString =
                                ((GenderEnum)gender == GenderEnum.Boy) ? "boys" : "girls";
                            growthResult!.Weight!.Description =
                                $"Your child is in the {percentile.Value} percentile for weight. That means {percentile.Value} percent of {genderString} at that age weigh less, while {formatPercentile(100 - percentile.Value)} percent weigh more.";

                            if (percentile.Value < 5)
                            {
                                growthResult!.Weight!.Level = LevelEnum.Low;
                                Console.WriteLine(
                                    $"[generateGrowthResult] Weight level set to Low"
                                );
                            }
                            else if (percentile.Value >= 5 && percentile.Value < 15)
                            {
                                growthResult!.Weight!.Level = LevelEnum.BelowAverage;
                                Console.WriteLine(
                                    $"[generateGrowthResult] Weight level set to BelowAverage"
                                );
                            }
                            else if (percentile.Value >= 15 && percentile.Value < 95)
                            {
                                growthResult!.Weight!.Level = LevelEnum.Average;
                                Console.WriteLine(
                                    $"[generateGrowthResult] Weight level set to Average"
                                );
                            }
                            else if (percentile.Value >= 95)
                            {
                                growthResult!.Weight!.Level = LevelEnum.AboveAverage;
                                irregular = true;
                                Console.WriteLine(
                                    $"[generateGrowthResult] Weight level set to AboveAverage (irregular)"
                                );
                            }
                        }
                        break;
                    }
                    case GrowthMetricsForAgeEnum.HCFA:
                    {
                        {
                            if (headCircumference == null || headCircumference is not double)
                            {
                                growthResult!.HeadCircumference!.Percentile = -1;
                                growthResult!.HeadCircumference!.Description = "N/A";
                                growthResult!.HeadCircumference!.Level = LevelEnum.NA;
                                break;
                            }
                        }

                        var percentile = GetPercentile(
                            (double)headCircumference!,
                            data.Percentiles.Values
                        );
                        growthResult!.HeadCircumference!.Percentile = percentile ?? -1;
                        var genderString =
                            ((GenderEnum)gender == GenderEnum.Boy) ? "boys" : "girls";
                        growthResult!.HeadCircumference!.Description =
                            $"Your child is in the {percentile} percentile for head circumference. That means {percentile} percent of {genderString} at that age have a smaller head, while {formatPercentile(100 - percentile ?? 0)} percent have a larger head.";

                        if (percentile < 5)
                        {
                            growthResult!.HeadCircumference!.Level = LevelEnum.Low;
                        }
                        else if (percentile >= 5 && percentile < 15)
                        {
                            growthResult!.HeadCircumference!.Level = LevelEnum.BelowAverage;
                        }
                        else if (percentile >= 15 && percentile < 95)
                        {
                            growthResult!.HeadCircumference!.Level = LevelEnum.Average;
                        }
                        else if (percentile >= 95)
                        {
                            growthResult!.HeadCircumference!.Level = LevelEnum.AboveAverage;
                        }
                        break;
                    }
                    case GrowthMetricsForAgeEnum.ACFA:
                    {
                        if (armCircumference == null || (armCircumference is not double))
                        {
                            growthResult!.ArmCircumference!.Percentile = -1;
                            growthResult!.ArmCircumference!.Description = "N/A";
                            growthResult!.ArmCircumference!.Level = LevelEnum.NA;
                            break;
                        }
                        var percentile = GetPercentile(
                            (double)armCircumference!,
                            data.Percentiles.Values
                        );
                        growthResult!.ArmCircumference!.Percentile = percentile ?? -1;
                        var genderString =
                            ((GenderEnum)gender == GenderEnum.Boy) ? "boys" : "girls";
                        growthResult!.ArmCircumference!.Description =
                            $"Your child is in the {percentile} percentile for arm circumference. That means {percentile} percent of {genderString} at that age have a smaller arm, while {formatPercentile(100 - percentile ?? 0)} percent have a larger arm.";

                        if (percentile < 5)
                        {
                            growthResult!.ArmCircumference!.Level = LevelEnum.Low;
                        }
                        else if (percentile >= 5 && percentile < 15)
                        {
                            growthResult!.ArmCircumference!.Level = LevelEnum.BelowAverage;
                        }
                        else if (percentile >= 15 && percentile < 95)
                        {
                            growthResult!.ArmCircumference!.Level = LevelEnum.Average;
                        }
                        else if (percentile >= 95)
                        {
                            growthResult!.ArmCircumference!.Level = LevelEnum.AboveAverage;
                        }
                        break;
                    }
                }
            });
            var wflhData = await _wflhRepository.GetWflhData((double)height!, (int)gender);
            wflhData.ForEach(data =>
            {
                var percentile = GetPercentile((double)weight!, data.Percentiles.Values);
                growthResult!.WeightForLength!.Percentile = percentile ?? -1;
                var genderString = ((GenderEnum)gender == GenderEnum.Boy) ? "boys" : "girls";
                growthResult!.WeightForLength!.Description =
                    $"Your child is in the {percentile} percentile for weight for length. That means {percentile} percent of {genderString} at that age have a lower weight for length, while {formatPercentile(100 - percentile ?? 0)} percent have a higher weight for length.";

                if (percentile < 5)
                {
                    growthResult!.WeightForLength!.Level = LevelEnum.Low;
                }
                else if (percentile >= 5 && percentile < 15)
                {
                    growthResult!.WeightForLength!.Level = LevelEnum.BelowAverage;
                }
                else if (percentile >= 15 && percentile < 95)
                {
                    growthResult!.WeightForLength!.Level = LevelEnum.Average;
                }
                else if (percentile >= 95)
                {
                    growthResult!.WeightForLength!.Level = LevelEnum.AboveAverage;
                    irregular = true;
                }
            });
            return growthResult;
        }

        private async Task<List<GrowthVelocityResult>> calculateGrowthVelocity(
            ChildModel child,
            List<GrowthVelocityModel> oneMonthIncrementData
        )
        {
            var childGrowthData = await _growthDataRepository.GetAllGrowthDataByChildId(child.Id);

            var result = new List<GrowthVelocityResult>();
            foreach (var interval in oneMonthIncrementData)
            {
                var startDays = interval.FirstInterval.InDays;
                var endDays = interval.LastInterval.InDays;

                var startData = FindClosestGrowthData(
                    childGrowthData.ToList(),
                    startDays,
                    child.BirthDate
                );
                var endData = FindClosestGrowthData(
                    childGrowthData.ToList(),
                    endDays,
                    child.BirthDate
                );

                if (startData == null || endData == null)
                {
                    result.Add(
                        new GrowthVelocityResult
                        {
                            Period = FormatPeriod(
                                interval.FirstInterval.InMonths,
                                interval.LastInterval.InMonths
                            ),
                            StartDate = child.BirthDate.AddDays(interval.FirstInterval.InDays),
                            EndDate = child.BirthDate.AddDays(interval.LastInterval.InDays),
                            Height = new GrowthMetric
                            {
                                Percentile = -1,
                                Description = "N/A",
                                Level = LevelEnum.NA,
                            },
                            Weight = new GrowthMetric
                            {
                                Percentile = -1,
                                Description = "N/A",
                                Level = LevelEnum.NA,
                            },
                            HeadCircumference = new GrowthMetric
                            {
                                Percentile = -1,
                                Description = "N/A",
                                Level = LevelEnum.NA,
                            },
                        }
                    );
                    continue;
                }

                var timeDiffMonths =
                    (
                        endData.InputDate.ToUniversalTime().Ticks
                        - startData.InputDate.ToUniversalTime().Ticks
                    ) / (1000 * 3600 * 24 * 30.4375);

                var heightVelocity = CalculateMetricVelocity(
                    startData.Height,
                    endData.Height,
                    timeDiffMonths
                );
                var weightVelocity = CalculateMetricVelocity(
                    startData.Weight,
                    endData.Weight,
                    timeDiffMonths
                );
                var headCircumferenceVelocity = CalculateMetricVelocity(
                    startData.HeadCircumference,
                    endData.HeadCircumference,
                    timeDiffMonths
                );

                double? heightPercentile =
                    heightVelocity != null
                        ? GetPercentile(heightVelocity.Value, interval.Percentiles.Values)
                        : (double?)null;
                double? weightPercentile =
                    weightVelocity != null
                        ? GetPercentile(weightVelocity.Value, interval.Percentiles.Values)
                        : (double?)null;
                double? headCircumferencePercentile =
                    headCircumferenceVelocity != null
                        ? GetPercentile(
                            headCircumferenceVelocity.Value,
                            interval.Percentiles.Values
                        )
                        : (double?)null;

                var heightDescription =
                    heightPercentile != null
                        ? $"Your child is in the {formatPercentile(heightPercentile.Value)} percentile for height growth velocity. That means {formatPercentile(heightPercentile.Value)} percent of {((GenderEnum)child.Gender == GenderEnum.Boy ? "boys" : "girls")} at that age have a slower height growth velocity, while {formatPercentile(100 - heightPercentile.Value)} percent have a faster height growth velocity."
                        : "N/A";

                var weightDescription =
                    weightPercentile != null
                        ? $"Your child is in the {formatPercentile(weightPercentile.Value)} percentile for weight growth velocity. That means {formatPercentile(weightPercentile.Value)} percent of {((GenderEnum)child.Gender == GenderEnum.Boy ? "boys" : "girls")} at that age have a slower weight growth velocity, while {formatPercentile(100 - weightPercentile.Value)} percent have a faster weight growth velocity."
                        : "N/A";

                var headCircumferenceDescription =
                    headCircumferencePercentile != null
                        ? $"Your child is in the {formatPercentile(headCircumferencePercentile.Value)} percentile for head circumference growth velocity. That means {formatPercentile(headCircumferencePercentile.Value)} percent of {((GenderEnum)child.Gender == GenderEnum.Boy ? "boys" : "girls")} at that age have a slower head circumference growth velocity, while {formatPercentile(100 - headCircumferencePercentile.Value)} percent have a faster head circumference growth velocity."
                        : "N/A";

                result.Add(
                    new GrowthVelocityResult
                    {
                        Period =
                            interval.FirstInterval.InMonths.ToString()
                            + " - "
                            + interval.LastInterval.InMonths.ToString()
                            + " months",
                        StartDate = child.BirthDate.AddDays(interval.FirstInterval.InDays),
                        EndDate = child.BirthDate.AddDays(interval.LastInterval.InDays),
                        Height = new GrowthMetric
                        {
                            Percentile = heightPercentile ?? -1,
                            Description = heightDescription,
                            Level = DetermineLevel(heightPercentile ?? -1),
                        },
                        Weight = new GrowthMetric
                        {
                            Percentile = weightPercentile ?? -1,
                            Description = weightDescription,
                            Level = DetermineLevel(weightPercentile ?? -1),
                        },
                        HeadCircumference = new GrowthMetric
                        {
                            Percentile = headCircumferencePercentile ?? -1,
                            Description = headCircumferenceDescription,
                            Level = DetermineLevel(headCircumferencePercentile ?? -1),
                        },
                    }
                );
            }
            Console.WriteLine($"[calculateGrowthVelocity] Result: {result}");
            return result;
        }

        public async Task<List<GrowthVelocityResult>?> generateGrowthVelocityByChildId(
            UserInfo requesterInfo,
            string childId
        )
        {
            var requesterId = requesterInfo.UserId;
            var requesterRole = requesterInfo.Role;
            var user = await _userRepository.GetByIdAsync(requesterId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }
            var child = await _childRepository.GetByIdAsync(childId);
            if (child == null)
            {
                throw new KeyNotFoundException("Child not found");
            }

            if (
                child.GuardianId.ToString() != requesterId
                && Enum.Parse<RoleEnum>(requesterRole) == RoleEnum.User
            )
            {
                throw new KeyNotFoundException("Growth data not found");
            }
            var today = DateTime.UtcNow;
            var birth = child.BirthDate.ToUniversalTime();
            var diffInDays = TimeSpan.FromTicks(today.Ticks - birth.Ticks).TotalDays;

            var ageInDays = Math.Round(diffInDays);

            var oneMonthIncrementData = new List<GrowthVelocityModel>();
            if (ageInDays > 30.4375)
            {
                var growthVelocityData = await _growthVelocityRepository.GetGrowthVelocityData(
                    (int)child.Gender
                );
                var counter = 2;
                foreach (var data in growthVelocityData)
                {
                    if (counter > 12)
                        break;
                    if (
                        data.FirstInterval.InWeeks == 0 && data.LastInterval.InWeeks == 4
                        || data.FirstInterval.InWeeks == 4 && data.LastInterval.InMonths == 2
                        || data.FirstInterval.InMonths == counter
                            && data.LastInterval.InMonths == counter + 1
                    )
                    {
                        oneMonthIncrementData.Add(data);
                        if (
                            data.FirstInterval.InMonths == counter
                            && data.LastInterval.InMonths == counter + 1
                        )
                        {
                            counter++;
                        }
                    }
                }
            }
            var results = await calculateGrowthVelocity(child, oneMonthIncrementData);

            var childUpdate = await _childRepository.GetByIdAsync(childId);
            if (childUpdate == null)
            {
                throw new KeyNotFoundException("Child not found");
            }
            if (
                childUpdate.GuardianId.ToString() != requesterId
                && Enum.Parse<RoleEnum>(requesterRole) == RoleEnum.User
            )
            {
                throw new KeyNotFoundException("Growth data not found");
            }
            childUpdate.GrowthVelocityResult = results;
            await _childRepository.UpdateAsync(childId, childUpdate);

            return results;
        }

        public async Task<(GrowthDataModel, List<GrowthVelocityResult>)> CreateGrowthDataAsync(
            UserInfo requesterInfo,
            string childId,
            GrowthDataModel growthData
        )
        {
            try
            {
                var requesterId = requesterInfo.UserId;
                var requesterRole = requesterInfo.Role;
                var user = await _userRepository.GetByIdAsync(requesterId);
                if (user == null)
                {
                    throw new KeyNotFoundException("User not found");
                }

                var child = await _childRepository.GetByIdAsync(childId);
                if (child == null)
                {
                    throw new KeyNotFoundException("Child not found");
                }
                if (
                    child.GuardianId.ToString() != requesterId
                    && Enum.Parse<RoleEnum>(requesterRole) == RoleEnum.User
                )
                {
                    //privacy
                    throw new KeyNotFoundException("Child not found");
                }

                if ((RoleEnum)Enum.Parse<RoleEnum>(requesterRole) == RoleEnum.Doctor)
                {
                    throw new UnauthorizedAccessException("Forbidden");
                }

                var childGrowthData = await _growthDataRepository.GetAllGrowthDataByChildId(
                    childId
                );

                if (childGrowthData.Any(data => data.InputDate == growthData.InputDate))
                {
                    throw new Exception(
                        "Growth data of this date already exists. Try another input date or update existing growth data"
                    );
                }

                var growthResult = await generateGrowthResult(growthData, child);

                var newGrowthData = new GrowthDataModel
                {
                    ChildId = childId,
                    InputDate = growthData.InputDate,
                    Height = growthData.Height,
                    Weight = growthData.Weight,
                    Bmi = growthData.Weight * 10000 / (growthData.Height * growthData.Height),
                    ArmCircumference = growthData.ArmCircumference,
                    HeadCircumference = growthData.HeadCircumference,
                    GrowthResult = growthResult,
                };
                var createdGrowthData = await _growthDataRepository.CreateAsync(newGrowthData);
                var growthVelocity = await generateGrowthVelocityByChildId(requesterInfo, childId);

                return (createdGrowthData, growthVelocity);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<GrowthDataModel?> GetGrowthDataByIdAsync(
            string growthDataId,
            UserInfo requesterInfo
        )
        {
            try
            {
                var requesterId = requesterInfo.UserId;
                var requesterRole = requesterInfo.Role;
                var user = await _userRepository.GetByIdAsync(requesterId);
                if (user == null)
                {
                    throw new KeyNotFoundException("User not found");
                }

                var growthData = await _growthDataRepository.GetByIdAsync(growthDataId);
                if (growthData == null)
                {
                    throw new KeyNotFoundException("Growth data not found");
                }
                var child = await _childRepository.GetByIdAsync(growthData.ChildId.ToString());
                if (child == null)
                {
                    throw new KeyNotFoundException("Child not found");
                }
                if (
                    child.GuardianId.ToString() != requesterId
                    && Enum.Parse<RoleEnum>(requesterRole) == RoleEnum.User
                )
                {
                    throw new KeyNotFoundException("Growth data not found");
                }

                return growthData;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<PaginationResult<GrowthDataModel>> GetGrowthDataByChildIdAsync(
            string childId,
            UserInfo requesterInfo,
            QueryParams query
        )
        {
            try
            {
                var requesterId = requesterInfo.UserId;
                var requesterRole = requesterInfo.Role;
                var user = await _userRepository.GetByIdAsync(requesterId);
                if (user == null)
                {
                    throw new KeyNotFoundException("User not found");
                }

                var child = await _childRepository.GetByIdAsync(childId);
                if (child == null)
                {
                    throw new KeyNotFoundException("Child not found");
                }
                if (
                    child.GuardianId.ToString() != requesterId
                    && Enum.Parse<RoleEnum>(requesterRole) == RoleEnum.User
                )
                {
                    throw new KeyNotFoundException("Child not found");
                }
                var childGrowthData = await _growthDataRepository.GetGrowthDataByChildId(
                    childId,
                    query
                );

                return childGrowthData;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<bool> DeleteGrowthDataAsync(string growthDataId, UserInfo requesterInfo)
        {
            try
            {
                var requesterId = requesterInfo.UserId;
                var requesterRole = requesterInfo.Role;
                var user = await _userRepository.GetByIdAsync(requesterId);
                if (user == null)
                {
                    throw new KeyNotFoundException("User not found");
                }

                if (requesterRole == "Doctor")
                {
                    throw new UnauthorizedAccessException("Forbidden");
                }
                var growthData = await _growthDataRepository.GetByIdAsync(growthDataId);
                if (growthData == null)
                {
                    throw new KeyNotFoundException("Growth data not found");
                }

                var child = await _childRepository.GetByIdAsync(growthData.ChildId.ToString());
                if (child == null)
                {
                    throw new KeyNotFoundException("Child not found");
                }
                if (
                    child.GuardianId.ToString() != requesterId
                    && Enum.Parse<RoleEnum>(requesterRole) == RoleEnum.User
                )
                {
                    throw new KeyNotFoundException("Growth data not found");
                }
                await _growthDataRepository.DeleteAsync(growthDataId);
                return true;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<GrowthDataModel?> UpdateGrowthDataAsync(
            string growthDataId,
            UserInfo requesterInfo,
            GrowthDataModel updateData
        )
        {
            try
            {
                var requesterId = requesterInfo.UserId;
                var requesterRole = requesterInfo.Role;
                var user = await _userRepository.GetByIdAsync(requesterId);
                if (user == null)
                {
                    throw new KeyNotFoundException("User not found");
                }

                if (requesterRole == "Doctor")
                {
                    throw new UnauthorizedAccessException("Forbidden");
                }

                GrowthDataModel? growthData = null;
                if (Enum.Parse<RoleEnum>(requesterRole) == RoleEnum.Admin)
                {
                    growthData = await _growthDataRepository.GetByIdAsync(growthDataId);
                }
                else if (Enum.Parse<RoleEnum>(requesterRole) == RoleEnum.User)
                {
                    growthData = await _growthDataRepository.GetByIdAsync(growthDataId);
                }
                else
                {
                    throw new KeyNotFoundException("Growth data not found");
                }

                if (growthData == null)
                {
                    throw new KeyNotFoundException("Growth data not found");
                }

                var child = await _childRepository.GetByIdAsync(growthData.ChildId.ToString());
                if (child == null)
                {
                    throw new KeyNotFoundException("Child not found");
                }

                if (
                    child.GuardianId.ToString() != requesterId
                    && Enum.Parse<RoleEnum>(requesterRole) == RoleEnum.User
                )
                {
                    throw new KeyNotFoundException("Growth data not found");
                }

                if (updateData.InputDate != null)
                {
                    var oldInputDate = growthData.InputDate.ToUniversalTime().Ticks;
                    var newInputDate = updateData.InputDate.ToUniversalTime().Ticks;

                    if (oldInputDate != newInputDate)
                    {
                        var childGrowthData = await _growthDataRepository.GetAllGrowthDataByChildId(
                            child.Id.ToString()
                        );
                        foreach (var data in childGrowthData)
                        {
                            if (
                                data.Id.ToString() != growthDataId
                                && data.InputDate != null
                                && data.InputDate.ToUniversalTime().Ticks == newInputDate
                            )
                            {
                                throw new InvalidOperationException(
                                    "Growth data of this date already exists. Try another input date or update existing growth data"
                                );
                            }
                        }
                    }
                }

                // Ensure nullable doubles are converted to non-nullable doubles with default value (0.0) if null
                if (updateData.Height != null && updateData.Height > 0)
                    growthData.Height = updateData.Height;
                if (updateData.Weight != null && updateData.Weight > 0)
                    growthData.Weight = updateData.Weight;
                if (updateData.HeadCircumference != null && updateData.HeadCircumference > 0)
                    growthData.HeadCircumference = updateData.HeadCircumference;
                if (updateData.ArmCircumference != null && updateData.ArmCircumference > 0)
                    growthData.ArmCircumference = updateData.ArmCircumference;
                if (updateData.Height != null || updateData.Weight != null)
                    growthData.Bmi =
                        updateData.Weight * 10000 / (growthData.Height * growthData.Height);

                // Recalculate growth result
                var growthResult = await generateGrowthResult(growthData, child);
                growthData.GrowthResult = growthResult;
                var updatedGrowthData = await _growthDataRepository.UpdateAsync(
                    growthDataId,
                    growthData
                );

                if (updatedGrowthData == null)
                {
                    throw new KeyNotFoundException("Growth data not found or cannot be updated");
                }

                return updatedGrowthData;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<GrowthResult?> PublicGenerateGrowthDataAsync(
            GrowthDataModel growthData,
            DateTime birthDate,
            int gender
        )
        {
            try
            {
                var growthDataResult = await generateGrowthResult2(growthData, birthDate, gender);
                return growthDataResult;
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}
