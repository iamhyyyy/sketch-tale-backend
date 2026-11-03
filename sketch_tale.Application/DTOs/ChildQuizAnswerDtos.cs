using System;
using System.Collections.Generic;

namespace sketch_tale.Application.DTOs;

public class CreateChildQuizAnswerDto
{
    public Guid ReadingLogId { get; set; }
    public Guid StoryQuizTemplateId { get; set; }
    public Guid? ChildProfileId { get; set; }
    public int SelectedOptionIndex { get; set; }
    public int ResponseTimeSeconds { get; set; }
}

public class ChildQuizAnswerDto : BaseFieldDto
{
    public Guid ChildProfileId { get; set; }
    public Guid ReadingLogId { get; set; }
    public Guid StoryQuizTemplateId { get; set; }

    public int QuizScore { get; set; }
    public int SelectedOptionIndex { get; set; }
    public bool IsCorrect { get; set; }
    public int ResponseTimeSeconds { get; set; }

    // Chi tiết câu hỏi để FE hiển thị lại
    public string? QuestionText { get; set; }
    public string? QuestionAudioUrl { get; set; }
    public string? QuestionPicUrl { get; set; }
    public List<string> Options { get; set; } = new();
    public int CorrectOptionIndex { get; set; }
}

public class SubmitQuizAnswerResponseDto : BaseFieldDto
{
    public Guid ChildProfileId { get; set; }
    public Guid ReadingLogId { get; set; }
    public Guid StoryQuizTemplateId { get; set; }

    public int SelectedOptionIndex { get; set; }
    public int CorrectOptionIndex { get; set; }
    public bool IsCorrect { get; set; }
    public int QuizScore { get; set; }
    public int ResponseTimeSeconds { get; set; }
    public string Message { get; set; } = string.Empty;

    // Tiến độ bài kiểm tra trả về ngay sau khi bé nộp câu này
    public int CurrentAnsweredCount { get; set; }      // Số câu đã làm xong (vd: 2)
    public int TotalQuestions { get; set; }             // Tổng số câu của truyện (vd: 4)
    public double CompletionPercentage { get; set; }    // % tiến độ bài làm (vd: 50.0%)
    public bool IsQuizCompleted { get; set; }           // Đã hoàn thành tất cả câu chưa
    public int Stars { get; set; }                      // 0 sao nếu chưa làm xong, 1 - 3 sao khi đã làm hết bài
}

public class ReadingLogQuizSummaryDto
{
    public Guid ReadingLogId { get; set; }
    public Guid ChildProfileId { get; set; }
    
    // Tiến độ hiển thị theo %
    public int TotalQuestions { get; set; }             // Tổng số câu hỏi
    public int TotalAnswered { get; set; }              // Số câu bé đã làm
    public int TotalCorrectAnswers { get; set; }        // Số câu đúng
    public double CompletionPercentage { get; set; }    // % tiến độ làm bài (vd: 50.0%)
    public double AccuracyPercentage { get; set; }      // % trả lời đúng (vd: 80.0%)
    public bool IsQuizCompleted { get; set; }           // Đã làm hết tất cả các câu chưa
    
    public int Stars { get; set; }                      // Đánh giá sao (0 sao nếu chưa xong bài, 1 - 3 sao khi làm xong)
    public List<ChildQuizAnswerDto> Answers { get; set; } = new();
}
