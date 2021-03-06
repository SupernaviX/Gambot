﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Modules.People
{
    internal class GenderCommandProducer : IMessageProducer
    {
        public enum Gender
        {
            Androgynous,
            Male,
            Female,
            Inanimate
        }

        private readonly IVariableHandler variableHandler;
        protected IDataStore genderStore;

        internal GenderCommandProducer(IVariableHandler variableHandler)
        {
            this.variableHandler = variableHandler;
        }

        protected Gender GetGender(string who)
        {
            var gender = genderStore.GetAllValues(who).Select(dsv => dsv.Value).SingleOrDefault() ??
                         default(Gender).ToString();
            return (Gender)Enum.Parse(typeof(Gender), gender, true);
        }

        public void Initialize(IDataStoreManager dataStoreManager)
        {
            genderStore = dataStoreManager.Get("Genders");

            var subjectivePronouns = new Dictionary<Gender, string>
            {
                {Gender.Androgynous, "they"},
                {Gender.Male, "he"},
                {Gender.Female, "she"},
                {Gender.Inanimate, "it"}
            };

            var objectivePronouns = new Dictionary<Gender, string>
            {
                {Gender.Androgynous, "them"},
                {Gender.Male, "him"},
                {Gender.Female, "her"},
                {Gender.Inanimate, "it"}
            };

            var reflexivePronouns = new Dictionary<Gender, string>
            {
                {Gender.Androgynous, "themself"},
                {Gender.Male, "himself"},
                {Gender.Female, "herself"},
                {Gender.Inanimate, "itself"}
            };

            var possessivePronouns = new Dictionary<Gender, string>
            {
                {Gender.Androgynous, "theirs"},
                {Gender.Male, "his"},
                {Gender.Female, "hers"},
                {Gender.Inanimate, "its"}
            };

            var possessiveDeterminers = new Dictionary<Gender, string>
            {
                {Gender.Androgynous, "their"},
                {Gender.Male, "his"},
                {Gender.Female, "her"},
                {Gender.Inanimate, "its"}
            };

            var subjectiveHandler =
                new Func<IMessage, string>(
                    (IMessage context) =>
                    subjectivePronouns[
                        GetGender(KnownPeopleListener.LastReferencedPerson)]);
            var objectiveHandler =
                new Func<IMessage, string>(
                    (IMessage context) =>
                    objectivePronouns[
                        GetGender(KnownPeopleListener.LastReferencedPerson)]);
            var reflexiveHandler =
                new Func<IMessage, string>(
                    (IMessage context) =>
                    reflexivePronouns[
                        GetGender(KnownPeopleListener.LastReferencedPerson)]);
            var possessiveHandler =
                new Func<IMessage, string>(
                    (IMessage context) =>
                    possessivePronouns[
                        GetGender(KnownPeopleListener.LastReferencedPerson)]);
            var possessiveDHandler =
                new Func<IMessage, string>(
                    (IMessage context) =>
                    possessiveDeterminers[
                        GetGender(KnownPeopleListener.LastReferencedPerson)]);

            foreach (
                var pronoun in
                    new[]
                    {"subjective", "shehe", "heshe", "he", "she", "they", "it"})
                variableHandler.DefineMagicVariable(pronoun, subjectiveHandler);
            foreach (
                var pronoun in
                    new[]
                    {"objective", "him", "her", "them", "himher", "herhim"})
                variableHandler.DefineMagicVariable(pronoun, objectiveHandler);
            foreach (
                var pronoun in
                    new[]
                    {
                        "reflexive", "himselfherself", "herselfhimself", "himself",
                        "herself", "themself", "itself"
                    })
                variableHandler.DefineMagicVariable(pronoun, reflexiveHandler);
            foreach (
                var pronoun in
                    new[] {"possessive", "hishers", "hershis", "hers", "theirs"}
                )
                variableHandler.DefineMagicVariable(pronoun, possessiveHandler);
            foreach (
                var pronoun in new[] {"determiner", "hisher", "herhis", "their"}
                )
                variableHandler.DefineMagicVariable(pronoun, possessiveDHandler);
        }

        public ProducerResponse Process(IMessage message, bool addressed)
        {
            if (addressed)
            {
                var personalMatch = Regex.Match(message.Text,
                                                @"^I am (androgynous|male|female|inanimate)[.!?]?$",
                                                RegexOptions.IgnoreCase);
                if (personalMatch.Success)
                {
                    genderStore.RemoveAllValues(message.Who);
                    genderStore.Put(message.Who, personalMatch.Groups[1].Value);
                    return
                        new ProducerResponse(
                            String.Format("Okay, {0}.", message.Who), false);
                }
            }

            return null;
        }
    }
}
