using FellowOakDicom;
using DicomViewer.Models;

namespace DicomViewer.Services;

public class DicomFileService : IDicomFileService
{
    public async Task<DicomFile?> LoadDicomFileAsync(string filePath)
    {
        try
        {
            return await DicomFile.OpenAsync(filePath);
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<DicomFile>> LoadDicomSeriesAsync(string[] filePaths)
    {
        var dicomFiles = new List<DicomFile>();

        foreach (var filePath in filePaths)
        {
            var dicomFile = await LoadDicomFileAsync(filePath);
            if (dicomFile != null)
            {
                dicomFiles.Add(dicomFile);
            }
        }

        return dicomFiles;
    }

    public PatientInfo ExtractPatientInfo(DicomFile dicomFile)
    {
        var dataset = dicomFile.Dataset;

        return new PatientInfo
        {
            PatientName = dataset.GetSingleValueOrDefault(DicomTag.PatientName, string.Empty),
            PatientID = dataset.GetSingleValueOrDefault(DicomTag.PatientID, string.Empty),
            BirthDate = dataset.GetSingleValueOrDefault(DicomTag.PatientBirthDate, string.Empty),
            Sex = dataset.GetSingleValueOrDefault(DicomTag.PatientSex, string.Empty)
        };
    }

    public StudyInfo ExtractStudyInfo(DicomFile dicomFile)
    {
        var dataset = dicomFile.Dataset;

        return new StudyInfo
        {
            StudyInstanceUID = dataset.GetSingleValueOrDefault(DicomTag.StudyInstanceUID, string.Empty),
            StudyDate = dataset.GetSingleValueOrDefault(DicomTag.StudyDate, string.Empty),
            StudyDescription = dataset.GetSingleValueOrDefault(DicomTag.StudyDescription, string.Empty),
            InstitutionName = dataset.GetSingleValueOrDefault(DicomTag.InstitutionName, string.Empty),
            Modality = dataset.GetSingleValueOrDefault(DicomTag.Modality, string.Empty)
        };
    }

    public SeriesInfo ExtractSeriesInfo(List<DicomFile> dicomFiles)
    {
        if (dicomFiles.Count == 0)
        {
            return new SeriesInfo();
        }

        var firstFile = dicomFiles[0];
        var dataset = firstFile.Dataset;

        // 시리즈 정보 추출
        var seriesInfo = new SeriesInfo
        {
            SeriesInstanceUID = dataset.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, string.Empty),
            SeriesNumber = dataset.GetSingleValueOrDefault(DicomTag.SeriesNumber, 0),
            SeriesDescription = dataset.GetSingleValueOrDefault(DicomTag.SeriesDescription, string.Empty)
        };

        // InstanceNumber로 정렬
        var sortedFiles = dicomFiles.OrderBy(f =>
            f.Dataset.GetSingleValueOrDefault(DicomTag.InstanceNumber, 0)
        ).ToList();

        return seriesInfo;
    }

    public List<DicomTagInfo> ExtractAllTags(DicomFile dicomFile)
    {
        var tags = new List<DicomTagInfo>();
        var dataset = dicomFile.Dataset;

        foreach (var item in dataset)
        {
            try
            {
                var tag = item.Tag;
                var vr = item.ValueRepresentation.Code;
                var value = dataset.GetValueOrDefault(tag, 0, string.Empty);

                tags.Add(new DicomTagInfo
                {
                    Tag = tag.ToString(),
                    VR = vr,
                    Value = value?.ToString() ?? string.Empty
                });
            }
            catch
            {
                // 태그 읽기 실패 시 건너뛰기
                continue;
            }
        }

        return tags;
    }
}
