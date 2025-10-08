<details>
<summary>English version 🇺🇸</summary>

<h1 align="center">
  



  <img src="https://github.com/user-attachments/assets/54f21caa-4fdb-4cb8-a282-104bda580d30" alt="FCG Logo" width="180">
  



  FIAP Cloud Games (FCG)
  



</h1>

<h4 align="center">
A scalable digital games platform, developed throughout the Tech Challenges of the <a href="https://www.fiap.com.br/" target="_blank">FIAP</a> Postgraduate Program in .NET Software Architecture.
</h4>

<p align="center">
  <a href="#-project-evolution">✨ Project Evolution</a> •
  <a href="#-key-technologies--concepts">🧠 Key Technologies & Concepts</a> •
  <a href="#-how-to-use">🚀 How to Use</a>
</p>

<hr>

<h2>✨ Project Evolution</h2>
This project was developed in three distinct phases, reflecting the evolution from a simple MVP to a complex, distributed, and cloud-native architecture.

<h3>Phase 1: Foundation and MVP (Monolith)</h3>
<p>The initial goal was to build the core of the platform. We developed a monolithic API in .NET 8 that included essential functionalities such as user registration and authentication with JWT. We applied Domain-Driven Design (DDD) principles and created a solid foundation with unit tests to ensure software quality from the start.</p>

<h3>Phase 2: Automation and Cloud Scalability</h3>
<p>With the MVP validated, the focus shifted to operational efficiency and scalability. The monolithic application was containerized using Docker. We implemented CI/CD pipelines to automate the testing and deployment processes, publishing the application to a cloud environment (AWS/Azure). We also integrated a monitoring stack to ensure the platform's reliability and performance.</p>

<h3>Phase 3: Microservices Architecture</h3>
<p>In the final phase, we evolved the architecture to a microservices model to increase modularity and resilience. The monolith was decomposed into independent services (Users, Games, Payments). We implemented Elasticsearch for advanced game searches and adopted serverless functions for asynchronous processes, all managed through an API Gateway, resulting in a robust and modern distributed system.</p>

<h2>🧠 Key Technologies & Concepts</h2>

<ul>
<li><strong>Backend</strong>: .NET 8 (Minimal APIs / MVC)</li>
<li><strong>Architecture</strong>: Monolith (Phases 1-2) ➔ Microservices (Phase 3), Domain-Driven Design (DDD), Event Sourcing</li>
<li><strong>Data Persistence</strong>: Entity Framework Core, Elasticsearch</li>
<li><strong>DevOps</strong>: Docker, CI/CD (GitHub Actions / Azure DevOps)</li>
<li><strong>Cloud</strong>: AWS / Azure, Serverless (Lambda / Functions), API Gateway</li>
<li><strong>Authentication</strong>: JWT (JSON Web Tokens)</li>
<li><strong>Software Quality</strong>: Unit Tests (TDD / BDD)</li>
<li><strong>Observability</strong>: Structured Logging, Distributed Tracing, Monitoring (Prometheus, Grafana, etc.)</li>
</ul>

<h2>🚀 How to Use</h2>

<p><em>(Note: The final project structure is based on microservices. Each service is in its own repository and has specific setup instructions in its respective README.)</em></p>

<ol>
<li><strong>Clone the repositories</strong>:
    <pre><code># Clone each microservice repository
git clone [Link to User Microservice Repository]
git clone [Link to Game Microservice Repository]
git clone [Link to Payment Microservice Repository]
    </code></pre>
</li>
<li><strong>Configure Environment Variables</strong>: Each microservice requires its own <code>.env</code> file. Refer to the <code>.env.example</code> in each repository to configure database connections, API keys, etc.</li>
<li><strong>Run the Infrastructure</strong>: Use <code>docker-compose up -d</code> to start necessary services like databases, Elasticsearch, and message queues.</li>
<li><strong>Run the Microservices</strong>: Navigate into each microservice's directory and run its start command (e.g., <code>dotnet run</code>).</li>
<li><strong>Access the Application</strong>: The services are exposed through the API Gateway. Use the Gateway's URL to interact with the complete application.</li>
</ol>

</details>

<h1 align="center">
  



  <img src="https://github.com/user-attachments/assets/54f21caa-4fdb-4cb8-a282-104bda580d30" alt="FCG Logo" width="180">
  



  FIAP Cloud Games (FCG)
  



</h1>

<h4 align="center">
Plataforma de jogos digitais escalável, desenvolvida ao longo dos Tech Challenges da Pós-graduação <a href="https://www.fiap.com.br/" target="_blank">FIAP</a> em Arquitetura de Software .NET.
</h4>

<p align="center">
  <a href="#-evolução-do-projeto">✨ Evolução do Projeto</a> •
  <a href="#-principais-tecnologias-e-conceitos">🧠 Principais Tecnologias e Conceitos</a> •
  <a href="#-como-usar">🚀 Como Usar</a>
</p>

<hr>

<h2>✨ Evolução do Projeto</h2>
Este projeto foi desenvolvido em três fases distintas, refletindo a evolução de um MVP simples para uma arquitetura complexa, distribuída e nativa da nuvem.

<h3>FASE 1: Fundação e MVP (Monolito)</h3>
<p>O objetivo inicial foi construir o núcleo da plataforma. Desenvolvemos uma API monolítica em .NET 8 que contemplava funcionalidades essenciais como cadastro e autenticação de usuários com JWT. Aplicamos princípios de Domain-Driven Design (DDD) e criamos uma base sólida com testes unitários para garantir a qualidade do software desde o início.</p>

<h3>FASE 2: Automação e Escalabilidade na Cloud</h3>
<p>Com o MVP validado, o foco mudou para a eficiência operacional e escalabilidade. A aplicação monolítica foi conteinerizada com Docker. Implementamos pipelines de CI/CD para automatizar os processos de teste e deploy, publicando a aplicação em um ambiente na nuvem (AWS/Azure). Integramos também uma stack de monitoramento para garantir a confiabilidade e o desempenho da plataforma.</p>

<h3>FASE 3: Arquitetura de Microsserviços</h3>
<p>Na fase final, evoluímos a arquitetura para um modelo de microsserviços, visando aumentar a modularidade e a resiliência. O monolito foi decomposto em serviços independentes (Usuários, Jogos, Pagamentos). Implementamos o Elasticsearch para buscas avançadas de jogos e adotamos funções serverless para processos assíncronos, tudo gerenciado por um API Gateway, resultando em um sistema distribuído, robusto e moderno.</p>

<h2>🧠 Principais Tecnologias e Conceitos</h2>

<ul>
<li><strong>Backend</strong>: .NET 8 (Minimal APIs / MVC)</li>
<li><strong>Arquitetura</strong>: Monolito (Fases 1-2) ➔ Microsserviços (Fase 3), Domain-Driven Design (DDD), Event Sourcing</li>
<li><strong>Persistência de Dados</strong>: Entity Framework Core, Elasticsearch</li>
<li><strong>DevOps</strong>: Docker, CI/CD (GitHub Actions / Azure DevOps)</li>
<li><strong>Cloud</strong>: AWS / Azure, Serverless (Lambda / Functions), API Gateway</li>
<li><strong>Autenticação</strong>: JWT (JSON Web Tokens)</li>
<li><strong>Qualidade de Software</strong>: Testes Unitários (TDD / BDD)</li>
<li><strong>Observabilidade</strong>: Logs Estruturados, Rastreamento Distribuído (Traces), Monitoramento (Prometheus, Grafana, etc.)</li>
</ul>

<h2>🚀 Como Usar</h2>

<p><em>(Nota: A estrutura final do projeto é baseada em microsserviços. Cada serviço está em seu próprio repositório e possui instruções de setup específicas em seu respectivo README.)</em></p>

<ol>
<li><strong>Clone os repositórios</strong>:
    <pre><code># Clone cada repositório dos microsserviços
git clone [Link para o repositório do Microsserviço de Usuários]
git clone [Link para o repositório do Microsserviço de Jogos]
git clone [Link para o repositório do Microsserviço de Pagamentos]
    </code></pre>
</li>
<li><strong>Configure as Variáveis de Ambiente</strong>: Cada microsserviço exige um arquivo <code>.env</code> próprio. Consulte o <code>.env.example</code> em cada repositório para configurar conexões de banco de dados, chaves de API, etc.</li>
<li><strong>Execute a Infraestrutura</strong>: Utilize <code>docker-compose up -d</code> para iniciar os serviços necessários como bancos de dados, Elasticsearch e filas de mensagens.</li>
<li><strong>Execute os Microsserviços</strong>: Navegue até o diretório de cada microsserviço e execute o seu comando de inicialização (ex: <code>dotnet run</code>).</li>
<li><strong>Acesse a Aplicação</strong>: Os serviços são expostos através do API Gateway. Utilize a URL do Gateway para interagir com a aplicação completa.</li>
</ol>
