-- Popula a tabela public.clientes com 200 registros ficticios.
-- Execute este script conectado ao banco ordem_servico_mvc
-- depois da criacao da estrutura.

INSERT INTO public.clientes (nome, telefone, email, criado_em)
SELECT
    initcap(
        primeiros_nomes[((n - 1) % array_length(primeiros_nomes, 1)) + 1]
        || ' ' ||
        sobrenomes[((n - 1) % array_length(sobrenomes, 1)) + 1]
        || ' ' ||
        sobrenomes[(((n - 1) / array_length(primeiros_nomes, 1)) % array_length(sobrenomes, 1)) + 1]
    ) AS nome,
    format(
        '(%s) 9%04s-%04s',
        lpad((((n - 1) % 27) + 11)::text, 2, '0'),
        lpad((((n * 37) % 10000))::text, 4, '0'),
        lpad((((n * 91) % 10000))::text, 4, '0')
    ) AS telefone,
    lower(
        replace(
            primeiros_nomes[((n - 1) % array_length(primeiros_nomes, 1)) + 1]
            || '.' ||
            sobrenomes[((n - 1) % array_length(sobrenomes, 1)) + 1]
            || n::text
            || '@exemplo.com',
            ' ',
            ''
        )
    ) AS email,
    now() - ((201 - n) || ' hours')::interval AS criado_em
FROM generate_series(1, 200) AS gs(n)
CROSS JOIN (
    SELECT
        ARRAY[
            'ana','bruno','carla','daniel','eduarda','felipe','gabriela','henrique','isabela','joao',
            'karina','lucas','mariana','nicolas','olivia','paulo','quezia','rafael','sabrina','thiago',
            'ursula','vinicius','william','ximena','yasmin','zeca','beatriz','caio','debora','emanuel',
            'fernanda','gustavo','helena','igor','juliana','leandro','milena','natalia','otavio','priscila'
        ]::text[] AS primeiros_nomes,
        ARRAY[
            'silva','souza','oliveira','pereira','costa','rodrigues','almeida','nascimento','lima','araujo',
            'fernandes','carvalho','gomes','martins','rocha','ribeiro','alves','monteiro','melo','barbosa'
        ]::text[] AS sobrenomes
) AS dados;
