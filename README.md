# ANNIE-

ANNIE+ is a .NET web application for online booking management. This application is intended for fitness studios to easily present available classes to their clients online. In turn, any online user will be able to view the company’s website, create an account, and book an available session for themselves.

Functionally, administrators (e.g., yoga studio owners or instructors) can:

* Add and delete sessions
* Customize session details (location, time, capacity, instructors, etc.)
* Modify session information after publication

All sessions appear in a **calendar view**, retrieved via the app’s API. Users can:

* View available sessions by date and time
* Select a session to see full details
* Sign in and book a spot

After booking:

* Users receive a confirmation email
* Session capacity updates automatically

Users can also:

* Manage bookings (cancel/view upcoming sessions)

Companies using ANNIE+ can:

* Check clients in
* Customize UI branding
* Choose between calendar or list views

Role-based login separates admin and user functionality.

---

## Non-Functional Requirements

The application emphasizes:

* **Security**:

  * Role-based data access
  * Encrypted data in transit

* **Performance**:

  * Efficient database queries
  * Pagination
  * Default monthly session retrieval
  * Rate limiting at 80% server capacity

* **Accessibility**:

  * Compliance with WCAG standards

---

## Market Context

Similar systems exist (e.g., MarinaTek), but ANNIE+ aims to:

* Improve UI flexibility
* Enhance security
* Expand usability to industries like:

  * Medical clinics
  * Airport services

---

# 2. Project Design

A booking application primarily manipulates database data. Unlike large-scale systems (e.g., concert ticketing), ANNIE+ focuses on:

* Simplicity
* Lightweight design
* Customizability

### Architecture Overview

The system consists of:

* **Frontend**: Blazor
* **Backend**: C# with RESTful APIs
* **Database**: PostgreSQL

### Data Flow

1. Frontend sends validated API requests
2. Backend processes requests via ASP.NET
3. Entity Framework handles database interaction
4. Models represent database structures

Key technologies:

* Entity Framework
* LINQ for querying
* DTOs for controlled data transfer

### Frontend Design

Blazor uses a **component-based architecture**:

* Reusable components
* HTML + inline C#
* Cleaner structure

Pagination is used for session display.

### Database Choice

PostgreSQL was selected because it is:

* Open-source
* Scalable
* Industry-standard

Integration is done via **Npgsql**.

---

# 3. Use Cases

## 3.1 Session Creation

**Description:**
An instructor creates a new session with all required details.

**Precondition:**

* Admin user is logged in

**Postcondition:**

* Session is correctly created and updated dynamically

**Error Case:**

* Invalid input data

**Actor:**

* Admin user

**Trigger:**

* Admin selects "Add Session"

---

## 3.2 Session Booking

**Description:**
A user books a session with available capacity.

**Precondition:**

* User is logged in
* Session is not full

**Postcondition:**

* Booking saved
* Confirmation email sent
* Capacity updated

**Error Cases:**

* Invalid payment
* Simultaneous booking conflict
* Session already full

**Actor:**

* Registered user

**Trigger:**

* User selects and books a session

---

# 4. Design Choices

## Rendering Mode

Options considered:

* Server-side rendering
* Client-side rendering
* Hybrid

**Chosen:** Server-side rendering

* Faster initial load
* Suitable for infrequent updates

## Frontend Framework

Blazor was selected because:

* Uses C# (consistent with backend)
* Component modularity
* Easier maintenance

UI customization may use the **singleton pattern**.

## API Design

Chosen approach: **RESTful APIs**

* Standard and widely used
* Simpler than GraphQL/gRPC

Implementation: **Controller-based APIs**

* Structured
* Supports single-responsibility principle

## Database Design

PostgreSQL + relational schema with four tables:

* **Sessions** (with status enum)
* **Users** (with role enum + hashed password)
* **Enrollments** (handles race conditions)
* **Locations** (address + capacity)

---

# 5. Conclusion

ANNIE+ is a web booking application designed for:

* Fitness studios
* Medical clinics
* Other service-based industries

### Technology Stack

* Blazor frontend
* C# backend with REST APIs
* PostgreSQL database

### Key Goals

* Security
* Efficiency
* Accessibility

### Challenges

* Handling race conditions in bookings
* Ensuring database consistency

### Testing Plan

* CRUD operations
* Authentication security
* Concurrency handling

### Milestones

* **Interim demo**: Basic functionality working
* **Final demo**:

  * Role-based login
  * Restricted data access
  * Email notifications

---

## Future Improvements

* Stronger security enforcement
* Secure payment system
* Saved payment methods
* Embeddable iframe integration

These improvements would make ANNIE+ a **production-ready system** comparable to existing solutions like MarinaTek.
