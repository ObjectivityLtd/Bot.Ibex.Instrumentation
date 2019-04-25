﻿namespace Bot.Ibex.Instrumentation.V4.Adapters
{
    using System;
    using System.Globalization;
    using Common.Models;

    public class QueryResultAdapter
    {
        private readonly Microsoft.Bot.Builder.AI.QnA.QueryResult queryResult;

        public QueryResultAdapter(Microsoft.Bot.Builder.AI.QnA.QueryResult queryResult)
        {
            this.queryResult = queryResult ?? throw new ArgumentNullException(nameof(queryResult));
        }

        public QueryResult ConvertQnAMakerResultsToQueryResult()
        {
            var result = new QueryResult
            {
                KnowledgeBaseQuestion = string.Join(QuestionsSeparator.Separator, this.queryResult.Questions),
                KnowledgeBaseAnswer = this.queryResult.Answer,
                Score = this.queryResult.Score.ToString(CultureInfo.InvariantCulture)
            };

            return result;
        }
    }
}
