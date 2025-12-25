using FellowOakDicom;
using DicomViewer.Models;

namespace DicomViewer.Services;

public interface IDicomFileService
{
    Task<DicomFile?> LoadDicomFileAsync(string filePath);
    Task<List<DicomFile>> LoadDicomSeriesAsync(string[] filePaths);
    PatientInfo ExtractPatientInfo(DicomFile dicomFile);
    StudyInfo ExtractStudyInfo(DicomFile dicomFile);
    SeriesInfo ExtractSeriesInfo(List<DicomFile> dicomFiles);
    List<DicomTagInfo> ExtractAllTags(DicomFile dicomFile);
}
