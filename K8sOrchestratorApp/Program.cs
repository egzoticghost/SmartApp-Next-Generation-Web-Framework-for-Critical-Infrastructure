using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using LibGit2Sharp;

// --- KONFIGURACJA ---
string repoUrl = "https://github.com/radzenhq/radzen-blazor.git";
string localPath = Path.Combine(Environment.CurrentDirectory, "source-code");
string nsName = "app";

// Definicje aplikacji
var apps = new[] {
    new { Name = "smart-api", Folder = "Backend", Image = "smart-api:latest", Port = 8080 },
    new { Name = "smart-blazor", Folder = "Frontend", Image = "smart-blazor:latest", Port = 80 }
};

Console.WriteLine("🚀 Start Orkiestratora...");

// --- 1. GIT: POBIERANIE KODU ---
if (Directory.Exists(localPath)) Directory.Delete(localPath, true);
Console.WriteLine("📥 Klonowanie repozytorium...");
Repository.Clone(repoUrl, localPath);

// --- 2. KUBERNETES: INICJALIZACJA KLIENTA ---
var config = KubernetesClientConfiguration.BuildDefaultConfig();
var client = new Kubernetes(config);

// --- 3. KUBERNETES: TWORZENIE NAMESPACE (Jeśli nie istnieje) ---
await EnsureNamespaceExists(client, nsName);

// --- 4. PĘTLA DEPLOYMENTU (Dla Frontu i Backendu) ---
foreach (var app in apps)
{
    Console.WriteLine($"\n--- Przetwarzanie: {app.Name} ---");

    // A. Docker Build
    Console.WriteLine($"🐳 Budowanie obrazu {app.Image}...");
    BuildDockerImage(app.Image, Path.Combine(localPath, app.Folder));

    // B. Tworzenie Deploymentu (Upsert)
    var deployment = CreateDeploymentObject(app.Name, app.Image, app.Port);
    await UpsertDeployment(client, deployment, nsName);

    // C. Tworzenie Serwisu (Upsert)
    var service = CreateServiceObject(app.Name, app.Port);
    await UpsertService(client, service, nsName);
}

Console.WriteLine("\n✅ Wszystkie komponenty zostały wdrożone!");

// --- METODY POMOCNICZE (LOGIKA) ---

void BuildDockerImage(string imageName, string contextPath)
{
    var process = Process.Start(new ProcessStartInfo
    {
        FileName = "docker",
        Arguments = $"build -t {imageName} {contextPath}",
        RedirectStandardOutput = true,
        UseShellExecute = false
    });
    process.WaitForExit();
}

async Task EnsureNamespaceExists(IKubernetes client, string name)
{
    var nsList = await client.CoreV1.ListNamespaceAsync();
    if (!nsList.Items.Any(n => n.Metadata.Name == name))
    {
        await client.CoreV1.CreateNamespaceAsync(new V1Namespace { Metadata = new V1ObjectMeta { Name = name } });
        Console.WriteLine($"✨ Utworzono namespace: {name}");
    }
}

async Task UpsertDeployment(IKubernetes client, V1Deployment body, string ns)
{
    try
    {
        await client.AppsV1.CreateNamespacedDeploymentAsync(body, ns);
        Console.WriteLine($"📦 Utworzono Deployment: {body.Metadata.Name}");
    }
    catch
    {
        await client.AppsV1.ReplaceNamespacedDeploymentAsync(body, body.Metadata.Name, ns);
        Console.WriteLine($"🔄 Zaktualizowano Deployment: {body.Metadata.Name}");
    }
}

async Task UpsertService(IKubernetes client, V1Service body, string ns)
{
    try
    {
        await client.CoreV1.CreateNamespacedServiceAsync(body, ns);
        Console.WriteLine($"🔌 Utworzono Serwis: {body.Metadata.Name}");
    }
    catch
    {
        // Serwisy są trudniejsze do Replace, czasem lepiej usunąć i stworzyć
        await client.CoreV1.DeleteNamespacedServiceAsync(body.Metadata.Name, ns);
        await client.CoreV1.CreateNamespacedServiceAsync(body, ns);
        Console.WriteLine($"🔄 Zrestartowano Serwis: {body.Metadata.Name}");
    }
}

V1Deployment CreateDeploymentObject(string name, string image, int port) => new V1Deployment
{
    Metadata = new V1ObjectMeta { Name = name },
    Spec = new V1DeploymentSpec
    {
        Replicas = 1,
        Selector = new V1LabelSelector { MatchLabels = new Dictionary<string, string> { { "app", name } } },
        Template = new V1PodTemplateSpec
        {
            Metadata = new V1ObjectMeta { Labels = new Dictionary<string, string> { { "app", name } } },
            Spec = new V1PodSpec
            {
                Containers = new List<V1Container> {
                    new V1Container { Name = "container", Image = image, Ports = new List<V1ContainerPort> { new V1ContainerPort(port) } }
                }
            }
        }
    }
};

V1Service CreateServiceObject(string name, int port) => new V1Service
{
    Metadata = new V1ObjectMeta { Name = name },
    Spec = new V1ServiceSpec
    {
        Selector = new Dictionary<string, string> { { "app", name } },
        Ports = new List<V1ServicePort> { new V1ServicePort(80, targetPort: port) },
        Type = "ClusterIP"
    }
};