# Sistem de Navigare (Navigation System Simulation)

## 🇷🇴 Descriere Proiect (Project Description - Romanian)

**Sistem de Navigare** este o aplicație desktop dezvoltată în C# (.NET Framework) ce simulează funcționalitățile de bază ale unui sistem de navigație. Aplicația permite utilizatorilor să introducă o locație de plecare și una de sosire, să vizualizeze rute posibile pe o hartă interactivă și să obțină estimări privind timpul de parcurs și consumul de combustibil.

Acest proiect a fost dezvoltat în cadrul disciplinei Ingineria Programării (Iași 2025).

## 🇬🇧 Project Description (English)

**Sistem de Navigare** (Navigation System) is a desktop application developed in C# (.NET Framework) that simulates the basic functionalities of a navigation system. The application allows users to input a starting location and a destination, view possible routes on an interactive map, and obtain estimates for travel time and fuel consumption.

This project was developed as part of the Software Engineering course (Iași 2025).

---

## 🖼️ Interfață Utilizator (User Interface)

Interfața grafică principală (`Form1`) include:
*   Câmpuri text pentru "Locație Plecare", "Locație Sosire" și "Viteză (km/h)".
*   Butonul "Calculează Ruta".
*   Controlul de hartă `gmapControl` pentru afișarea hărții și a rutelor.
*   O listă (`listBox1`) pentru afișarea și selecția rutelor alternative.
*   Câmpuri text (doar citire) pentru "Timp estimat" și "Consum estimat".
*   Meniu "Help" pentru acces la documentația de ajutor.



## ✨ Funcționalități Principale (Key Features)

*   **Introducere Locații:** Utilizatorii pot introduce locațiile de plecare și sosire sub formă de text.
*   **Geocodare:** Adresele introduse sunt transformate în coordonate geografice folosind serviciul Nominatim (OpenStreetMap).
*   **Calculare și Afișare Rute:**
    *   Calculează una sau mai multe rute alternative folosind serviciul OSRM (Open Source Routing Machine).
    *   Afișează rutele pe o hartă interactivă (GMap.NET) cu culori distincte.
    *   Listează rutele alternative, permițând selecția uneia dintre ele.
*   **Evidențiere Rută Selectată:** Ruta selectată de utilizator este evidențiată pe hartă.
*   **Calcul Metrici Călătorie:**
    *   Permite introducerea opțională a vitezei medii de deplasare.
    *   Calculează și afișează timpul estimat de parcurs și consumul estimat de combustibil pentru ruta selectată (consum implicit: 7.5 L/100km, viteză implicită: 60 km/h).
*   **Managementul Stării Interfeței:** Utilizează State Design Pattern pentru a gestiona stările UI (ex: `InitialState`, `LoadingState`, `RoutesDisplayedState`, `RouteSelectedState`).
*   **Notificări Utilizator:** Afișează mesaje de eroare, avertismente sau stări ale sistemului prin ferestre de dialog (`MessageBox`).
*   **Sistem de Ajutor:** Oferă un fișier de ajutor în format CHM accesibil din meniul "Help".

## 🛠️ Tehnologii Utilizate (Technologies Used)

*   **Limbaj de Programare:** C#
*   **Platformă:** .NET Framework 4.8
*   **Interfață Grafică:** Windows Forms
*   **Control Hartă:** GMap.NET
*   **Serviciu de Rutare:** API OSRM (Open Source Routing Machine)
*   **Serviciu de Geocodare:** API Nominatim (OpenStreetMap)
*   **Parsare JSON:** Newtonsoft.Json (sau System.Text.Json)
*   **Arhitectură:** Modulară, cu funcționalități separate în DLL-uri:
    *   `RoutingServiceDLL.dll` (interacțiune cu API OSRM)
    *   `RouteCalculatorDLL.dll` (calcul metrici călătorie)
    *   `ColorNameDLL.dll` (generare nume culori pentru rute)
*   **Design Pattern:** State Design Pattern (pentru managementul stărilor UI)

## 📋 Cerințe (Prerequisites)

*   Sistem de operare Microsoft Windows care suportă .NET Framework 4.8.
*   Conexiune activă la internet (pentru interogarea serviciilor OSRM și Nominatim).
*   Bibliotecile GMap.NET și Newtonsoft.Json (acestea ar trebui să fie incluse/distribuite cu aplicația sau gestionate via NuGet).

## 🚀 Utilizare (Usage)

1.  Lansați aplicația.
2.  Introduceți locația de plecare în câmpul "Locație Plecare".
3.  Introduceți locația de sosire în câmpul "Locație Sosire".
4.  Opțional, introduceți o viteză medie în câmpul "Viteză (km/h)". Dacă nu este specificată, se va folosi o valoare implicită (60 km/h).
5.  Apăsați butonul "Calculează Ruta".
6.  Aplicația va afișa rutele pe hartă și în lista de rute. Metricele pentru prima rută (sau ruta implicită) vor fi afișate.
7.  Selectați o rută din listă pentru a o evidenția pe hartă și pentru a actualiza metricile afișate (timp estimat, consum estimat).
8.  Pentru ajutor, accesați meniul "Help".

## 🔧 Structura Proiectului (Project Structure - based on DLLs)

*   **Aplicația Principală (Proiect WinForms):** Conține interfața utilizator (`Form1.cs`) și logica de gestionare a stărilor (`ApplicationState.cs` și implementările stărilor).
*   **RoutingServiceDLL.dll:** Responsabilă pentru comunicarea cu API-ul OSRM, obținerea datelor de rutare și decodarea geometriei polyline.
*   **RouteCalculatorDLL.dll:** Calculează timpul estimat de parcurs și consumul de combustibil pe baza datelor rutei și a vitezei.
*   **ColorNameDLL.dll:** Utilizată pentru a atribui nume de culori distincte rutelor afișate pe hartă.

## 🌐 Servicii Externe (External Services)

*   **OSRM (Open Source Routing Machine):** Folosit pentru calculul rutelor.
    *   URL: `http://router.project-osrm.org/`
*   **Nominatim (OpenStreetMap):** Folosit pentru geocodarea adreselor text în coordonate geografice.
    *   URL: `https://nominatim.openstreetmap.org/`

## 🧑‍💻 Autori (Authors)

**Studenți (Grupa 1312A):**
*   Chilimon Ana-Maria
*   Chiriac Raluca-Ștefania
*   Gălusca Mihnea-Ioan
*   Stoian Mario-Daniel

**Coordonator:**
*   Prof. Florin Leon

---
