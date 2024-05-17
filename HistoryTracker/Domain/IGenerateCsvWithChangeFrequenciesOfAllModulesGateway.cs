﻿
using Domain.MetaData;

namespace Domain
{
    public interface IGenerateCsvWithChangeFrequenciesOfAllModulesGateway
    {
        bool CreateCsvFileWithChangeFrequenciesOfModules(ICollection<ChangeFrequency> changeFrequenciesOfModules, string csvFilePath);
    }
}