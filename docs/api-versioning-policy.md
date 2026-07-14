# TMS API Versioning Policy

This document defines how the Training Management System (TMS) API guarantees compatibility, manages evolutionary changes, and schedules deprecation paths.

## 1. What Counts as a Breaking Change
The following changes alter the expected structure of active endpoints and require a **Major Version Increment (e.g., V1 → V2)**:
*   **Response Payload Structure:** Renaming fields, altering default types, or omitting existing JSON fields.
*   **Behavior and Querying:** Restructuring the pagination metadata locations or altering default sort schemas.
*   **Request Validation:** Tightening validation requirements or introducing mandatory request headers/fields.
*   **Error Codes:** Changing standard HTTP Response Status codes.

## 2. What Counts as an Additive (Non-Breaking) Change
Additive improvements will be introduced transparently **without changing the API version number**:
*   Adding new optional properties to requests or responses.
*   Adding entirely new entity endpoint roots or routes.
*   Adding optional query filtering capabilities.

## 3. Sunset and Deprecation Strategy
When a new major version becomes primary, the deprecated path remains active for a strict support grace period:
*   **Sunset Horizon:** Old major versions are kept operational for at least **6 months** post-deprecation date.
*   **Proactive Signals:** Every request to the deprecated engine immediately receives headers indicating sunset timelines (`Deprecation: true`, `Sunset`, `Link: successor-version`).

## 4. Multi-Channel Version Negotiation
Consumers may access explicit API targets using:
1.  **URI Path Segment (Default Strategy):** `/api/v1/courses` vs `/api/v2/courses`
2.  **HTTP Request Header (Partner Integration):** `X-Api-Version: 2.0` targeted on non-versioned base URIs.