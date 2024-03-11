
const express = require('express');
const Consul = require('consul');
const app = express();

app.get('/health', (req, res) => {
    res.status(200).json({ status: 'TESTANDO SERVICE REGISTRY' });
});

app.get('/api/marvel', (req, res) => {
    res.status(200).json({ character: 'Wolverine' });
});

app.get('/api/genres', (req, res) => {
    res.status(200).json({
        "genres": [
          {"id": 1, "name": "Ficção Científica"},
          {"id": 2, "name": "Fantasia"},
          {"id": 3, "name": "Romance"},
          {"id": 4, "name": "Terror"},
          {"id": 5, "name": "Biografia"},
          {"id": 6, "name": "História"},
          {"id": 7, "name": "Autoajuda"},
          {"id": 8, "name": "Educação"},
          {"id": 9, "name": "Economia"},
          {"id": 10, "name": "Saúde"}
        ]
      });
});


const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
    console.log(`Servidor rodando na porta ${PORT}`);
    const serviceName = "genre-service-api";
    const servicePort = 443; // Porta do seu serviço

    const consul = new Consul({
        host: "consul.jeffs.dev",
        port: 4433,
        secure: true,
        // Defina como true se estiver urrrsando HTTPS
        // Outras opções avançadas, como tls, agent, etc.
    });

    const ngrokHost = "db5b-2804-7f0-b341-7523-a0cd-1b48-1dc6-a8d4.ngrok-free.app"
    try{
        consul.agent.service.deregister({
            id: serviceName,
    
        }, (err, result) => {
            if (err) {
                console.error("Erro ao registrar serviço:", err);
            } else {
                console.log("Serviço registrado com sucesso:", result);
            }
        });
    }catch(error){
        let message = 'erro ao desregistrar serviço'
        if (error.message){
            message = error.message
            console.log(message)
        }
        console.log(message)
    }
    consul.agent.service.register({
        node: ngrokHost,
        name: serviceName,
        address: ngrokHost,
        port: servicePort,
        // check: {
        //     http: `https://${ngrokHost}:${PORT}/health`,
        //     interval: '10s',
        //     timeout: '5s'
        //   }
        // Outras opções, como tags, health check, etc.r
    }, (err, result) => {
        if (err) {
            console.error("Erro ao registrar serviço:", err);
        } else {
            console.log("Serviço registrado com sucesso:", result);
        }
    });

});





