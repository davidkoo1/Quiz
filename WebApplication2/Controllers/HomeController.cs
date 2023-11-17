﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using WebApplication2.Models;
using WebApplication2.Models.API;
using WebApplication2.ViewModels;

namespace WebApplication2.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        public async Task<IActionResult> Index()
        {
            string token = GetToken();
            using (var httpClient = new HttpClient())
            {

                var apiUrlForGetProfileInfo = "https://dev.edi.md/ISAuthService/json/GetProfileInfo?Token=" + token;

                var responseGetProfileInfo = await httpClient.GetAsync(apiUrlForGetProfileInfo);

                if (responseGetProfileInfo.IsSuccessStatusCode)
                {
                    // Чтение данных из HTTP-ответа.

                    var userData = await responseGetProfileInfo.Content.ReadAsAsync<GetProfileInfo>();
                    if (userData.ErrorCode == 143)
                    {
                        await RefreshToken();
                        return await Index();
                    }
                    else if (userData.ErrorCode == 118)
                    {
                        return View("~/Views/Account/Login.cshtml");
                    }
                    else if (userData.ErrorCode == 0)
                    {
                        using (var httpClient1 = new HttpClient())
                        {

                            var apiUrlGetQuestionnairesByToken = "https://dev.edi.md/ISNPSAPI/Web/GetQuestionnaires?token=" + token;
                            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("uSr_nps:V8-}W31S!l'D"));

                            // Добавляем аутентификацию в заголовок Authorization с префиксом "Basic ".
                            httpClient1.DefaultRequestHeaders.Add("Authorization", "Basic " + credentials);


                            var responseGetQuestionnaires = await httpClient1.GetAsync(apiUrlGetQuestionnairesByToken);

                            if (responseGetQuestionnaires.IsSuccessStatusCode)
                            {
                                var questionnaireData = await responseGetQuestionnaires.Content.ReadAsAsync<GetQuestionnaireInfo>();
                                if (questionnaireData.errorCode == 143)
                                {
                                    await RefreshToken();
                                    return await Index();
                                }
                                else if (questionnaireData.errorCode == 118)
                                {
                                    return View("~/Views/Account/Login.cshtml");
                                }
                                else if (questionnaireData.errorCode == 0)
                                {

                                    return View("~/Views/Home/Index.cshtml", questionnaireData.questionnaires);

                                }



                            }
                        }
                    }



                }
            }




            return View("~/Views/Home/Index.cshtml");
        }


        public async Task<IActionResult> Detail(int id)
        {
            string token = GetToken();
            using (var httpClient = new HttpClient())
            {

                var apiUrlForGetProfileInfo = "https://dev.edi.md/ISAuthService/json/GetProfileInfo?Token=" + token;

                var responseGetProfileInfo = await httpClient.GetAsync(apiUrlForGetProfileInfo);

                if (responseGetProfileInfo.IsSuccessStatusCode)
                {
                    // Чтение данных из HTTP-ответа.

                    var userData = await responseGetProfileInfo.Content.ReadAsAsync<GetProfileInfo>();
                    if (userData.ErrorCode == 143)
                    {
                        await RefreshToken();
                        return await Detail(id);
                    }
                    else if (userData.ErrorCode == 118)
                    {
                        return View("~/Views/Account/Login.cshtml");
                    }
                    else if (userData.ErrorCode == 0)
                    {
                        using (var httpClient1 = new HttpClient())
                        {

                            var apiUrlGetQuestionnaire = "https://dev.edi.md/ISNPSAPI/Web/GetQuestionnaire?Token=" + token + "&id=" + id;
                            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("uSr_nps:V8-}W31S!l'D"));

                            // Добавляем аутентификацию в заголовок Authorization с префиксом "Basic ".
                            httpClient1.DefaultRequestHeaders.Add("Authorization", "Basic " + credentials);


                            var responseGetQuestionnaire = await httpClient1.GetAsync(apiUrlGetQuestionnaire);

                            if (responseGetQuestionnaire.IsSuccessStatusCode)
                            {
                                var questionnaireData = await responseGetQuestionnaire.Content.ReadAsAsync<DetailQuestionnaire>();
                                if (questionnaireData.errorCode == 143)
                                {
                                    await RefreshToken();
                                    return await Detail(id);
                                }
                                else if (questionnaireData.errorCode == 118)
                                {
                                    return View("~/Views/Account/Login.cshtml");
                                }
                                else if (questionnaireData.errorCode == 0)
                                {
                                    var upsertVm = new CreateQuestionnaireViewModel();
                                    upsertVm.oid = id;
                                    upsertVm.Title = questionnaireData.questionnaire.name;
                                    upsertVm.Questions = questionnaireData.questionnaire.questions;
                                    return /*Partial*/View("~/Views/Home/Detail.cshtml", upsertVm);

                                }
                            }
                        }
                    }
                }
            }
            return View("Error");
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateQuestionnaireViewModel CreateQuestionnaireVM)
        {
            if (CreateQuestionnaireVM.Questions != null)
            {
                string token = GetToken();
                using (var httpClient = new HttpClient())
                {

                    var apiUrlForGetProfileInfo = "https://dev.edi.md/ISAuthService/json/GetProfileInfo?Token=" + token;

                    var responseGetProfileInfo = await httpClient.GetAsync(apiUrlForGetProfileInfo);

                    if (responseGetProfileInfo.IsSuccessStatusCode)
                    {
                        // Чтение данных из HTTP-ответа.

                        var userData = await responseGetProfileInfo.Content.ReadAsAsync<GetProfileInfo>();
                        if (userData.ErrorCode == 143)
                        {
                            await RefreshToken();
                            return await Create(CreateQuestionnaireVM);
                        }
                        else if (userData.ErrorCode == 118)
                        {
                            return View("~/Views/Account/Login.cshtml");
                        }
                        else if (userData.ErrorCode == 0)
                        {
                            using (var httpClient1 = new HttpClient())
                            {
                                var apiUrlForPostQuestionnaire = "https://dev.edi.md/ISNPSAPI/Web/UpsertQuestionnaire";
                                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("uSr_nps:V8-}W31S!l'D"));

                                // Добавляем аутентификацию в заголовок Authorization с префиксом "Basic ".
                                httpClient1.DefaultRequestHeaders.Add("Authorization", "Basic " + credentials);
                                //Correct model for post
                                CreateQuestionnaire createQuestionnaire = new CreateQuestionnaire();
                                createQuestionnaire.oid = 0;
                                createQuestionnaire.name = CreateQuestionnaireVM.Title;
                                createQuestionnaire.questions = CreateQuestionnaireVM.Questions;
                                /*List<CreateQuestion> tmp = new List<CreateQuestion>();
                                foreach (var question in CreateQuestionnaireVM.Questions)
                                {
                                    CreateQuestion createQuestion = new CreateQuestion();
                                    createQuestion.question = question.Text;
                                    createQuestion.comentary = question.comentary;
                                    createQuestion.gradingType = question.gradingType;
                                    if (question.Answers.Any())
                                    {
                                        createQuestion.answerVariants = new List<string>();
                                        foreach (var answer in question.Answers)
                                        {
                                            //if (answer.Text != null)
                                            createQuestion.answerVariants.Add(answer);
                                        }
                                    }
                                    tmp.Add(createQuestion);
                                }
                                createQuestionnaire.questions = tmp;
                                */

                                createQuestionnaire.companyOid = userData.User.CompanyID;
                                createQuestionnaire.token = token;
                                createQuestionnaire.company = userData.User.Company;

                                var jsonContent = new StringContent(JsonConvert.SerializeObject(createQuestionnaire), Encoding.UTF8, "application/json");

                                var responsePostQuestionnaire = await httpClient1.PostAsync(apiUrlForPostQuestionnaire, jsonContent);

                                if (responsePostQuestionnaire.IsSuccessStatusCode)
                                {
                                    // Чтение данных из HTTP-ответа.

                                    var questionnaireBaseResponsedData = await responsePostQuestionnaire.Content.ReadAsAsync<BaseErrors>(); //Тут была ошибка из-за названия переменных

                                    if (questionnaireBaseResponsedData.errorCode == 143)
                                    {
                                        await RefreshToken();
                                        return await Create(CreateQuestionnaireVM); ///!
                                    }
                                    else if (questionnaireBaseResponsedData.errorCode == 0)
                                    {
                                        return RedirectToAction(nameof(HomeController.Index), "Home");
                                    }



                                }
                            }
                        }
                    }
                }
            }

            return View("~/Views/Home/Index.cshtml");
        }


        //[HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            string token = GetToken();
            using (var httpClient = new HttpClient())
            {

                var apiUrlForGetProfileInfo = "https://dev.edi.md/ISAuthService/json/GetProfileInfo?Token=" + token;

                var responseGetProfileInfo = await httpClient.GetAsync(apiUrlForGetProfileInfo);

                if (responseGetProfileInfo.IsSuccessStatusCode)
                {
                    // Чтение данных из HTTP-ответа.

                    var userData = await responseGetProfileInfo.Content.ReadAsAsync<GetProfileInfo>();
                    if (userData.ErrorCode == 143)
                    {
                        await RefreshToken();
                        return await Edit(id);
                    }
                    else if (userData.ErrorCode == 118)
                    {
                        return View("~/Views/Account/Login.cshtml");
                    }
                    else if (userData.ErrorCode == 0)
                    {
                        using (var httpClient1 = new HttpClient())
                        {

                            var apiUrlGetQuestionnaire = "https://dev.edi.md/ISNPSAPI/Web/GetQuestionnaire?Token=" + token + "&id=" + id;
                            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("uSr_nps:V8-}W31S!l'D"));

                            // Добавляем аутентификацию в заголовок Authorization с префиксом "Basic ".
                            httpClient1.DefaultRequestHeaders.Add("Authorization", "Basic " + credentials);


                            var responseGetQuestionnaire = await httpClient1.GetAsync(apiUrlGetQuestionnaire);

                            if (responseGetQuestionnaire.IsSuccessStatusCode)
                            {
                                var questionnaireData = await responseGetQuestionnaire.Content.ReadAsAsync<DetailQuestionnaire>();
                                if (questionnaireData.errorCode == 143)
                                {
                                    await RefreshToken();
                                    return await Edit(id);
                                }
                                else if (questionnaireData.errorCode == 118)
                                {
                                    return View("~/Views/Account/Login.cshtml");
                                }
                                else if (questionnaireData.errorCode == 0)
                                {
                                    var upsertVm = new CreateQuestionnaireViewModel();
                                    upsertVm.oid = id;
                                    upsertVm.Title = questionnaireData.questionnaire.name;
                                    upsertVm.Questions = questionnaireData.questionnaire.questions;
                                    return /*Partial*/View("~/Views/Home/Edit.cshtml", upsertVm);

                                }
                            }
                        }
                    }
                }
            }
            return View("Error");
        }
        [HttpPost]
        public async Task<IActionResult> EditQuestionnaire([FromBody] EditQuestionnareViewModel editQuestionnaireVM)
        {

            var quest = JsonConvert.DeserializeObject<QuestionsViewModel>(editQuestionnaireVM.Questions);
            string token = GetToken();
            using (var httpClient = new HttpClient())
            {

                var apiUrlForGetProfileInfo = "https://dev.edi.md/ISAuthService/json/GetProfileInfo?Token=" + token;

                var responseGetProfileInfo = await httpClient.GetAsync(apiUrlForGetProfileInfo);

                if (responseGetProfileInfo.IsSuccessStatusCode)
                {
                    // Чтение данных из HTTP-ответа.

                    var userData = await responseGetProfileInfo.Content.ReadAsAsync<GetProfileInfo>();
                    if (userData.ErrorCode == 143)
                    {
                        await RefreshToken();
                        return await EditQuestionnaire(editQuestionnaireVM);
                    }
                    else if (userData.ErrorCode == 118)
                    {
                        return View("~/Views/Account/Login.cshtml");
                    }
                    else if (userData.ErrorCode == 0)
                    {
                        using (var httpClient1 = new HttpClient())
                        {
                            var apiUrlForPostQuestionnaire = "https://dev.edi.md/ISNPSAPI/Web/UpsertQuestionnaire";
                            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("uSr_nps:V8-}W31S!l'D"));

                            // Добавляем аутентификацию в заголовок Authorization с префиксом "Basic ".
                            httpClient1.DefaultRequestHeaders.Add("Authorization", "Basic " + credentials);
                            //Correct model for post
                            CreateQuestionnaire createQuestionnaire = new CreateQuestionnaire();
                            createQuestionnaire.oid = editQuestionnaireVM.id;
                            createQuestionnaire.name = editQuestionnaireVM.Title;
                            createQuestionnaire.questions = quest.questions;

                            createQuestionnaire.companyOid = userData.User.CompanyID;
                            createQuestionnaire.token = token;
                            createQuestionnaire.company = userData.User.Company;

                            var jsonContent = new StringContent(JsonConvert.SerializeObject(createQuestionnaire), Encoding.UTF8, "application/json");

                            var responsePostQuestionnaire = await httpClient1.PostAsync(apiUrlForPostQuestionnaire, jsonContent);

                            if (responsePostQuestionnaire.IsSuccessStatusCode)
                            {
                                // Чтение данных из HTTP-ответа.

                                var questionnaireBaseResponsedData = await responsePostQuestionnaire.Content.ReadAsAsync<BaseErrors>(); //Тут была ошибка из-за названия переменных

                                if (questionnaireBaseResponsedData.errorCode == 143)
                                {
                                    await RefreshToken();
                                    return await EditQuestionnaire(editQuestionnaireVM); ///!
                                }
                                else if (questionnaireBaseResponsedData.errorCode == 0)
                                {
                                    return Json(new { StatusCode = 200 });
                                }



                            }
                        }
                    }
                }
            }
            return View("~/Views/Home/Index.cshtml");
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {

            string token = GetToken();
            using (var httpClient = new HttpClient())
            {

                var apiUrlForGetProfileInfo = "https://dev.edi.md/ISAuthService/json/GetProfileInfo?Token=" + token;

                var responseGetProfileInfo = await httpClient.GetAsync(apiUrlForGetProfileInfo);

                if (responseGetProfileInfo.IsSuccessStatusCode)
                {
                    // Чтение данных из HTTP-ответа.

                    var userData = await responseGetProfileInfo.Content.ReadAsAsync<GetProfileInfo>();
                    if (userData.ErrorCode == 143)
                    {
                        await RefreshToken();
                        return await Delete(id);
                    }
                    else if (userData.ErrorCode == 118)
                    {
                        return View("~/Views/Account/Login.cshtml");
                    }
                    else if (userData.ErrorCode == 0)
                    {
                        using (var httpClient1 = new HttpClient())
                        {

                            var apiUrlGetQuestionnaire = "https://dev.edi.md/ISNPSAPI/Web/GetQuestionnaire?Token=" + token + "&id=" + id;
                            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("uSr_nps:V8-}W31S!l'D"));

                            // Добавляем аутентификацию в заголовок Authorization с префиксом "Basic ".
                            httpClient1.DefaultRequestHeaders.Add("Authorization", "Basic " + credentials);


                            var responseGetQuestionnaires = await httpClient1.GetAsync(apiUrlGetQuestionnaire);

                            if (responseGetQuestionnaires.IsSuccessStatusCode)
                            {
                                var questionnaireData = await responseGetQuestionnaires.Content.ReadAsAsync<DetailQuestionnaire>();
                                if (questionnaireData.errorCode == 143)
                                {
                                    await RefreshToken();
                                    return await Delete(id);
                                }
                                else if (questionnaireData.errorCode == 118)
                                {
                                    return View("~/Views/Account/Login.cshtml");
                                }
                                else if (questionnaireData.errorCode == 0)
                                {

                                    return PartialView("~/Views/Home/Delete.cshtml", questionnaireData);

                                }
                            }
                        }
                    }
                }
            }

            return PartialView();
        }
        //[HttpDelete, ActionName("Delete")]
        [HttpPost]
        public async Task<IActionResult> DeleteQuestionnaire([FromBody] int oid)
        {
            string token = GetToken();
            using (var httpClient = new HttpClient())
            {

                var apiUrlForGetProfileInfo = "https://dev.edi.md/ISAuthService/json/GetProfileInfo?Token=" + token;

                var responseGetProfileInfo = await httpClient.GetAsync(apiUrlForGetProfileInfo);

                if (responseGetProfileInfo.IsSuccessStatusCode)
                {
                    // Чтение данных из HTTP-ответа.

                    var userData = await responseGetProfileInfo.Content.ReadAsAsync<GetProfileInfo>();
                    if (userData.ErrorCode == 143)
                    {
                        await RefreshToken();
                        return await DeleteQuestionnaire(oid);
                    }
                    else if (userData.ErrorCode == 118)
                    {
                        return View("~/Views/Account/Login.cshtml");
                    }
                    else if (userData.ErrorCode == 0)
                    {
                        using (var httpClient1 = new HttpClient())
                        {

                            var apiUrlDeleteQuestionnaire = "https://dev.edi.md/ISNPSAPI/Web/DeleteQuestionnaire?Token=" + token + "&id=" + oid;
                            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("uSr_nps:V8-}W31S!l'D"));

                            // Добавляем аутентификацию в заголовок Authorization с префиксом "Basic ".
                            httpClient1.DefaultRequestHeaders.Add("Authorization", "Basic " + credentials);


                            var responseGetQuestionnaires = await httpClient1.DeleteAsync(apiUrlDeleteQuestionnaire);

                            if (responseGetQuestionnaires.IsSuccessStatusCode)
                            {
                                var questionnaireData = await responseGetQuestionnaires.Content.ReadAsAsync<BaseErrors>();
                                if (questionnaireData.errorCode == 143)
                                {
                                    await RefreshToken();
                                    return await DeleteQuestionnaire(oid);
                                }
                                else if (questionnaireData.errorCode == 118)
                                {
                                    return View("~/Views/Account/Login.cshtml");
                                }
                                else if (questionnaireData.errorCode == 0)
                                {

                                    // return PartialView("~/Views/Home/Index.cshtml");
                                    return Json(new { StatusCode = 200, Message = "Ok" });

                                }
                                else
                                {
                                    //TempData["Error"] = "Password unchenged";
                                    //return View("~/Views/User/Settings.cshtml");
                                    return Json(new { StatusCode = 500, Message = questionnaireData.errorMessage });
                                }



                            }
                        }
                    }



                }
            }
            return View("Index");
        }

    }
}
