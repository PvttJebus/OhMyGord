using System.Collections.Generic;

public interface IParameterEditable
{
    /// <summary>
    /// Export all editable parameters to a list of ParameterData for serialization.
    /// </summary>
    List<LevelData.ParameterData> ExportParameters();

    /// <summary>
    /// Import editable parameters from a list of ParameterData (deserialization).
    /// </summary>
    void ImportParameters(List<LevelData.ParameterData> parameters);
}