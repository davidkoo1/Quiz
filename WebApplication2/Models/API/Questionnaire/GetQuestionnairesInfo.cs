﻿namespace WebApplication2.Models.API
{
    public class GetQuestionnairesInfo : BaseErrors
    {
        //public string errorMessage { get; set; }
        //public string errorName { get; set; }
        //public int errorCode { get; set; }

        public List<Questionnaire> questionnaires { get; set; } //?null

    }
}
