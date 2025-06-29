# Report Service

Imagine you're asked to fix a report service that was originally built by a now-departed developer.  
All you know about the service is that it’s supposed to generate an accounting report for a selected month,  
listing all employees of the company. The report service has no other functionality.

---

## Sample Report

**January 2017**

---

### Finance Department

| Name              | Salary  |
|-------------------|---------|
| Andrew Barnes     | 2,200 € |
| Gregory Evans     | 2,000 € |
| Jacob Smith       | 2,500 € |
| Alex Ryan         | 2,700 € |

**Department Total: 9,400 €**

---

### Accounting

| Name                | Salary  |
|---------------------|---------|
| William Johnson     | 1,800 € |
| Damian Carter       | 2,000 € |
| Michael Anderson    | 1,500 € |

**Department Total: 5,300 €**

---

### IT

| Name               | Salary  |
|--------------------|---------|
| Philip Rogers      | 2,700 € |
| Dmitry Collins     | 3,500 € |
| Andrew Miller      | 3,200 € |
| Arvid Nelson       | 3,500 € |

**Department Total: 12,900 €**

---

### Company Total: **27,600 €**

---

## Background

The report service is already in production but behaves very unreliably. At some point, it stopped working altogether.  
Your task is to bring the report service back to life, fix known bugs, and clean up the project,  
as the previous developer was not very tidy in their work.

## Technical Note from Previous Developer

> The list of employees by department can be retrieved from the `employee` database.  
> An employee’s monthly salary can be obtained from the accounting department’s web service,  
> but you must provide it with the employee code from the HR service.

## Known Bugs (Reported by Users)

- Sometimes the system does not return the report and instead throws an error  
- It is very slow  
- Not all employees appear in the report  
- The “Company Total” line is missing

## Your Objectives

- Revive the report service  
- Fix all known issues  
- Refactor and organize the codebase  
- Add automated tests to avoid future breakdowns  

> ⚠️ You don't have access to the real database or web services, since the information is strictly confidential.  
> Mocks or stubs should be used for testing and development.