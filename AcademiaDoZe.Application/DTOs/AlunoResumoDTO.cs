namespace AcademiaDoZe.Application.DTOs;

// dados básicos de um aluno para listagens (ex.: alunos sem acesso recente)
public record AlunoResumoDTO(int Id, string Nome, string Cpf, string Email, string Telefone);
