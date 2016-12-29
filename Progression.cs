using System;

namespace Character
{
    /// <summary>
    /// System that manages levels and experience. Immutable. Uses C# 6 features
    /// </summary>
    public class Progression
    {
        public long Experience { get; } = 0;
        public long Level => (long) Math.Round((double) experienceToLevelFormula(Experience), 0, MidpointRounding.AwayFromZero);
        /// <summary>
        /// Gets the experience needed for next level up.
        /// </summary>
        /// <value>
        /// The experience until level up.
        /// </value>
        public long ExperienceUntilLevelUp => GetExperienceForLevel(Level + 1) - Experience;
        /// <summary>
        /// Gets the percentage until next level up
        /// </summary>
        /// <value>
        /// The percentage until level up.
        /// </value>
        /// <exception cref="Exception">Internal error: Evaluated percentage is not within range 0-100</exception>
        public long PercentageUntilLevelUp
        {
            get
            {
                var nextLevel = Level + 1;
                var ratio = Experience / GetExperienceForLevel(nextLevel);
                const long ratioToPercentage = 100;
                var percentage = (long)Math.Floor((decimal)(ratio * ratioToPercentage));
                if ((percentage < 0) || (percentage > 100))
                    throw new Exception("Internal error: Evaluated percentage is not within range 0-100");
                return percentage;
            }
        }
        private readonly Func<long, long> experienceToLevelFormula;

        /// <summary>
        /// Initializes a new instance of the <see cref="Progression"/> class.
        /// </summary>
        /// <param name="experienceToLevelFormula">Formula that returns level based on experience.</param>
        /// <exception cref="ArgumentException">The experience formula cannot be null</exception>
        public Progression(Func<long, long> experienceToLevelFormula)
        {
            if (experienceToLevelFormula == null)
            {
                throw new ArgumentException("The experience formula cannot be null");
            }
            this.experienceToLevelFormula = experienceToLevelFormula;
        }

        private Progression(Func<long, long> experienceToLevelFormula, long startExperience)
            : this(experienceToLevelFormula)
        {
            if(startExperience < 0)
                throw new ArgumentException("Can't create an instance of Progression with negative experience");
            Experience = startExperience;
        }

        /// <summary>
        /// The level the system is at given the ammount of experience
        /// </summary>
        /// <param name="experience">The experience.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Cannot expect a level for experience less than 0</exception>
        public long ExpectedLevel(long experience)
        {
            if (experience < 0)
                throw new ArgumentException("Cannot expect a level for experience less than 0");
            return experienceToLevelFormula(experience);
        }

        /// <summary>
        /// Returns the experience required to go from current level to the specified level. Includes current experience
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Level cannot be less than 0</exception>
        public long ExperienceFromCurrToXLevel(long level)
        {
            if(level < 0)
                throw new ArgumentException("Level cannot be less than 0");
            return Experience - GetExperienceForLevel(level);
        }

        /// <summary>
        /// Returns a new Progression instance with the added experience
        /// </summary>
        /// <param name="experience">The experience.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Experience added must be more than 0</exception>
        public Progression AddExperience(long experience)
        {
            if (experience < 0)
                throw new ArgumentException("Experience added must be more than 0");
            return ModifyExperience(experience, Sign.Plus);
        }

        /// <summary>
        /// Returns a new Progression instance with the removed experience
        /// </summary>
        /// <param name="experience">The experience.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Experience removed must be more than 0</exception>
        public Progression RemoveExperience(long experience)
        {
            if(experience < 0)
                throw new ArgumentException("Experience removed must be more than 0");
            return ModifyExperience(experience, Sign.Minus);
        }

        /// <summary>
        /// Returns a new Progression instance with the set level
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Level must be higher than 0</exception>
        public Progression SetLevel(long level)
        {
            if(level < 0)
                throw new ArgumentException("Level must be higher than 0");
            return new Progression(experienceToLevelFormula, GetExperienceForLevel(level));
        }

        /// <summary>
        /// Returns a new Progression instance with experience set to the current's instance's experience on last level up
        /// </summary>
        /// <returns></returns>
        public Progression ResetExperienceToCurrentLevel()
            => new Progression(experienceToLevelFormula, GetExperienceForLevel(this.Level));

        /// <summary>
        /// Gets the experience needed to be at level given.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        public long GetExperienceForLevel(long level)
        {
            //this function can be made more efficient
            long lowIndex = 0;
            var highIndex = long.MaxValue;
            var currTryIndex = lowIndex;
            long currTryEval = 0;
            bool isMoreMax = false;
            while ((currTryEval != level))
            {
                var range = highIndex - lowIndex;
                var maxVal = experienceToLevelFormula(highIndex);
                isMoreMax = maxVal > level;
                currTryIndex = lowIndex + range / 2;
                currTryEval = experienceToLevelFormula(currTryIndex);
                var isMoreTry = currTryEval > level;
                if (isMoreMax == isMoreTry)
                    highIndex = currTryIndex;
                else
                    lowIndex = currTryIndex;
            }
            long sign = isMoreMax ? 1 : -1;
            //this could be replaced with a binary search
            while (currTryEval == level)
            {
                currTryIndex -= sign;
                currTryEval = experienceToLevelFormula(currTryIndex);
            }
            currTryIndex += sign;
            return currTryIndex;
        }

        private Progression ModifyExperience(long experience, Sign sign)
        {
            long signMultiplier = sign == Sign.Plus ? 1 : -1;
            var newExperience = experience*signMultiplier;
            if (newExperience < 0)
                throw new Exception("Experience modification error: New experience is less than 0");
            return new Progression(experienceToLevelFormula, Experience);
        }

        private enum Sign
        {
            Plus,
            Minus
        }
    }
}
