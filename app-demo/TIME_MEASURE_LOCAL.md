# V0: Sem otimizações

### HEAP: 1GB
> iterations.....................: 228     
> - 6.194916/s

> ✗ statement_req_duration.........: 
> - avg=7343.142464 
> - min=335.9318 
> - med=7771.30145 
> - max=10254.4487 
> - p(90)=9246.82216 
> - p(95)=9379.91499

# V1: Sem lazy loading e com AsNoTracking() e Include()

### HEAP: 630MB
> iterations.....................: 471     
> - 12.604084/s

> ✗ statement_req_duration.........: 
> - avg=3568.719288 
> - min=359.8208 
> - med=3495.0029 
> - max=8486.5738 
> - p(90)=5430.6797 
> - p(95)=6073.82375

# V2: Banco indexado

### HEAP: 670MB
> iterations.....................: 602     
> - 16.701439/s

> ✗ statement_req_duration.........: 
> - avg=2746.069048 
> - min=102.1259 
> - med=1748.583 
> - max=14179.3243 
> - p(90)=7429.59664 
> - p(95)=10493.29559

# V3: Dapper

### HEAP: 502MB
> iterations.....................: 988     
> - 26.919816/s
   
> ✗ statement_req_duration.........: 
> - avg=1710.758127 
> - min=59.7489 
> - med=1601.77825 
> - max=7285.1065 
> - p(90)=3000.50985 
> - p(95)=3420.48206

# V4: Paginação
### HEAP: 110MB
> iterations.....................: 23668   
> - 675.157588/s
  
> ✓ statement_req_duration.........: 
> - avg=65.739479 
> - min=0.5082  
> - med=59.5605 
> - max=911.0828 
> - p(90)=92.79715 
> - p(95)=113.56212