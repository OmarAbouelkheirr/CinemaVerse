type ParamValue = string | number | boolean;

/**
 * Converts a filter object into a clean params record for ApiClientService.
 * Strips null, undefined, and empty-string values so they are never sent to the API.
 */
export function buildParams(filters: object): Record<string, ParamValue> {
  const params: Record<string, ParamValue> = {};

  for (const [key, value] of Object.entries(filters)) {
    if (value === null || value === undefined || value === '') {
      continue;
    }
    params[key] = value as ParamValue;
  }

  return params;
}
