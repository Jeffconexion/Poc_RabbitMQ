<h1 align="center">Poc about RabbitMQ</h1>
<p align="center"><i>Repository for versioning and project documentation used during the create Api.</i></p>

## About this project

### How it is structure

<p>
I maked a simple api. You can publish and consumer a mensager for rabbitmq.
</p>

### Run the project

<p>
You will need to run docker, create container rabbitmq.

<code> 
docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.10-management
</code>
</p>

<p>
You can access the rabbitmq at http://localhost:15672/, with the username and password guest.
</p>

### Architecture

<p display="inline-block">
  <img width="85%"  src="https://github.com/Jeffconexion/Poc_RabbitMQ/blob/main/Architecture.png" alt="vs-logo"/>
</p>
</p>

## References

[Luis Dev - Tutorial](https://www.luisdev.com.br/2022/07/06/introducao-ao-rabbitmq/)


