SELECT 
    Id,
    TaskName,
    Description,
    Due_Date,
    CreatedByID,
    CreatedOn,
    ModifiedByID,
    ModifiedOn
FROM Tasks
WHERE CAST(Due_Date AS DATE) = CAST(GETDATE() AS DATE);
