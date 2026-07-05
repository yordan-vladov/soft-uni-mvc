# Задание 1: Търсене, филтриране, сортиране и pagination

## Цел

Да се запознаете с проекта, да го стартирате локално и да пренесете функционалностите за
търсене, филтриране, сортиране и странициране (pagination), които вече съществуват в
Employees, към останалите index страници в приложението.

---

## Част 1: Подготовка на средата

1. **Подкарване на Git и GitHub**

- Инсталирайте [Git](https://git-scm.com), ако още не сте
- Направете си профил в [GitHub](https://github.com), ако още не сте

2. **Направете fork на хранилището и го клонирайте**

   Отворете [хранилището](https://github.com/yordan-vladov/soft-uni-mvc) в GitHub и натиснете бутона **Fork** горе вдясно, за да си
   направите собствено копие (fork) под вашия акаунт.

   ```bash
   git clone <адрес на вашия fork>
   cd soft-uni-mvc
   ```

3. **Пуснете базата данни**

   Нужен ви е локален MySQL сървър. Заредете предоставения SQL дъмп, за да се създаде и
   напълни базата `softuni`:

   ```bash
   mysql -u root -p < softuni.sql
   ```

4. **Задайте connection string**

   Създайте файл `appsettings.Development.json` в root-а на проекта (той е в `.gitignore`,
   така че паролата ви никога не се качва в git):

   ```json
   {
     "ConnectionStrings": {
       "SoftUniContext": "Server=localhost;Database=softuni;User=root;Password=yourpassword;"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     }
   }
   ```

5. **Възстановете пакетите и стартирайте приложението**

   ```bash
   dotnet restore
   dotnet run
   ```

   Отворете адреса, който се отпечатва в конзолата (по подразбиране
   `http://localhost:5206`). Уверете се, че можете да отворите страницата на Employees и
   да видите таблицата със служители, търсачката, филтъра по отдел и пагинацията.

Ако някоя от тези стъпки не работи (грешка при връзка с базата, липсващ SDK и т.н.), решете
проблема, преди да продължите към Част 2 — цялото задание разчита на работещо приложение.

---

## Част 2: Как работят функционалностите в Employees

Преди да пишете код, разгледайте внимателно тези три файла — те са референтната
имплементация, която ще копирате:

- [Controllers/EmployeesController.cs](Controllers/EmployeesController.cs) — метод `Index`
- [Views/Employees/Index.cshtml](Views/Employees/Index.cshtml)
- [Models/PaginatedList.cs](Models/PaginatedList.cs)

### 2.1 Търсене (search)

```csharp
public async Task<IActionResult> Index(string? searchString, int? departmentId, string? sortOrder, int? pageNumber)
```

`searchString` идва като GET параметър от формата в изгледа (`<input name="searchString" ...>`).
В контролера той се прилага като `WHERE` условие върху заявката, преди да заредим данните:

```csharp
if (!string.IsNullOrWhiteSpace(searchString))
{
    employees = employees.Where(e =>
        e.FirstName.Contains(searchString) ||
        e.LastName.Contains(searchString) ||
        e.MiddleName!.Contains(searchString) ||
        e.JobTitle.Contains(searchString));
}
```

Важното е, че заявката е от тип `IQueryable<T>` (не `List<T>`) — филтрите се добавят към нея
като допълнителни LINQ извиквания и EF Core ги превръща в един SQL `WHERE`, изпълнен в
базата данни, а не в паметта на приложението.

### 2.2 Филтриране (filter)

Филтърът по отдел работи по същия принцип, но със стойност от dropdown вместо свободен текст:

```csharp
ViewData["DepartmentFilter"] = new SelectList(_context.Departments.OrderBy(d => d.Name), "DepartmentId", "Name", departmentId);
...
if (departmentId.HasValue)
{
    employees = employees.Where(e => e.DepartmentId == departmentId);
}
```

`SelectList`-ът пълни `<select>`-а в изгледа с всички отдели, а третият аргумент
(`departmentId`) казва кой option да е избран, след като страницата се презареди — така
избраният филтър "остава" видимо избран.

### 2.3 Сортиране (sort)

Всяко заглавие на колона в таблицата е линк, който препраща обратно към `Index` с параметър
`sortOrder`:

```html
<a asp-action="Index" asp-route-sortOrder="@ViewData["NameSortParam"]" ...>Име</a>
```

В контролера, преди заявката, се изчислява каква ще е *следващата* стойност на сортиране за
всяка колона — ако вече сортираме по име възходящо, линкът за "Име" ще води до низходящо, и
обратно:

```csharp
ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
```

После `switch` изразът превръща текстовата стойност на `sortOrder` в конкретен `OrderBy`/
`OrderByDescending`:

```csharp
employees = sortOrder switch
{
    "name_desc" => employees.OrderByDescending(e => e.FirstName).ThenByDescending(e => e.LastName),
    "jobtitle" => employees.OrderBy(e => e.JobTitle),
    ...
    _ => employees.OrderBy(e => e.FirstName).ThenBy(e => e.LastName),
};
```

### 2.4 Странициране (pagination)

Класът [PaginatedList\<T\>](Models/PaginatedList.cs) обвива резултата: брои общия брой
редове (`CountAsync`), после взима само страницата, която трябва да се покаже
(`Skip`/`Take`):

```csharp
public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
{
    var count = await source.CountAsync();
    var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
    return new PaginatedList<T>(items, count, pageIndex, pageSize);
}
```

Извиква се едва **накрая**, след като всички филтри и сортировки вече са приложени към
`IQueryable`-а:

```csharp
const int pageSize = 10;
var paginatedEmployees = await PaginatedList<Employee>.CreateAsync(employees, pageNumber ?? 1, pageSize);
return View(paginatedEmployees);
```

Изгледът показва `PageIndex` и `TotalPages`, и генерира линкове "Previous"/"Next", които пазят
текущото търсене, филтър и сортиране (чрез `asp-route-*` атрибутите) — иначе преминаването на
страница би изгубило активния филтър.

**Защо редът е важен:** филтриране → сортиране → странициране. Ако странирате преди да
филтрирате, ще показвате грешни резултати (страница 2 от нефилтрирания списък, вместо от
филтрирания).

---

## Част 3: Задачата

Приложете същите четири функционалности (търсене, филтриране, сортиране, pagination) към
index страниците на:

- **Departments** ([Controllers/DepartmentsController.cs](Controllers/DepartmentsController.cs),
  [Views/Departments/Index.cshtml](Views/Departments/Index.cshtml))
- **Projects** ([Controllers/ProjectsController.cs](Controllers/ProjectsController.cs),
  [Views/Projects/Index.cshtml](Views/Projects/Index.cshtml))
- **Towns** ([Controllers/TownsController.cs](Controllers/TownsController.cs),
  [Views/Towns/Index.cshtml](Views/Towns/Index.cshtml))
- **Addresses** ([Controllers/AddressesController.cs](Controllers/AddressesController.cs),
  [Views/Addresses/Index.cshtml](Views/Addresses/Index.cshtml))
- **EmployeeProjects** ([Controllers/EmployeeProjectsController.cs](Controllers/EmployeeProjectsController.cs),
  [Views/EmployeeProjects/Index.cshtml](Views/EmployeeProjects/Index.cshtml))

За всяка страница, минимални изисквания:

| Страница | Търсене по | Филтър по | Сортиране по |
|---|---|---|---|
| Departments | Name | Manager (dropdown) | Name, Manager |
| Projects | Name, Description | — (по желание: активен/приключил проект спрямо `EndDate`) | Name, StartDate, EndDate |
| Towns | Name | — | Name |
| Addresses | AddressText | Town (dropdown) | AddressText, Town |
| EmployeeProjects | име на служител, име на проект | Project (dropdown) | Employee, Project |

Не всяка страница трябва да има филтър — там, където няма естествена колона за филтриране
(напр. Towns), е достатъчно търсене + сортиране + pagination.

### Стъпки за всяка страница

1. Променете `Index` action-а в контролера да приема параметрите
   `searchString`, `sortOrder`, евентуален филтър (напр. `managerId`, `townId`, `projectId`) и
   `pageNumber`.
2. Постройте `IQueryable`, приложете `Where` за търсене/филтър, `OrderBy`/`OrderByDescending`
   за сортиране (чрез `switch` по `sortOrder`), и накрая опаковайте резултата с
   `PaginatedList<T>.CreateAsync(...)`.
3. Сменете модела на изгледа от `List<T>`/`IEnumerable<T>` на `PaginatedList<T>`.
4. Добавете формата за търсене/филтър и линковете за сортиране в заглавията на таблицата,
   както е в `Views/Employees/Index.cshtml`.
5. Добавете `<nav>` с pagination в края на страницата, копирайки блока от
   `Views/Employees/Index.cshtml`, като не забравяте да пренесете всички текущи
   `asp-route-*` параметри (търсене, филтър, сортиране), за да не се губят при смяна на
   страницата.

### Критерии за приемане

- Всяка от петте index страници позволява търсене по текст, сортиране по поне 2 колони и
  разлистване на резултатите на страници (pagination), без да губи активното търсене/филтър
  при смяна на страница или сортиране.
- Търсенето и филтрирането се случват в базата данни (чрез `IQueryable`/`Where`), не чрез
  зареждане на всички редове и филтриране в паметта.
- Приложението компилира и стартира без грешки: `dotnet build`.
- Ръчно тествайте всяка страница в браузъра: търсене с празен резултат, сортиране в двете
  посоки, преминаване между страници, комбинация от търсене + филтър + сортиране едновременно.

### Съвети

- Не презаписвайте `EmployeesController`/`Employees/Index.cshtml` — те са референцията, към
  която да гледате, ако нещо не работи.
- `PaginatedList<T>` вече е aбстрактен клас — не се налага да го променяте, за да работи с
  `Department`, `Project`, `Town`, `Address` или `EmployeeProject`.
- За `EmployeeProjects`, тъй като няма собствено `Name` поле, търсенето трябва да мине през
  свързаните `Employee`/`Project` (`.Include(...)` вече е налично в контролера).
