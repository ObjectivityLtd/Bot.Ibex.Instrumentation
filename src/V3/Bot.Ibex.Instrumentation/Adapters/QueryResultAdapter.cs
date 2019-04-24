﻿namespace Bot.Ibex.Instrumentation.V3.Adapters
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Bot.Ibex.Instrumentation.Common.Instrumentations;
    using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;

    public class QueryResultAdapter
    {
        private readonly QnAMakerResults queryResult;

        public QueryResultAdapter(QnAMakerResults queryResult)
        {
            this.queryResult = queryResult ?? throw new ArgumentNullException(nameof(queryResult));
        }

        public QueryResult ConvertQnAMakerResultsToQueryResult()
        {
            var result = new QueryResult();
            var topScoreAnswer = this.queryResult.Answers.OrderByDescending(x => x.Score).First();

            result.KnowledgeBaseQuestion = string.Join(QnAInstrumentation.QuestionsSeparator, topScoreAnswer.Questions);
            result.KnowledgeBaseAnswer = topScoreAnswer.Answer;
            result.Score = topScoreAnswer.Score.ToString(CultureInfo.InvariantCulture);

            return result;
        }
    }
}
